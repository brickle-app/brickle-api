using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.DTOs.Relayer;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Domain.Models;
using BricklePlatform.Infrastructure.Services;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace BricklePlatform.Test.Services;

public class RelayerContractsCompileTests
{
    [Fact]
    public void RelayerDtosExposeExpectedPublicShape()
    {
        var request = new RelayerSponsorRequestDto
        {
            Command = "payment",
            Token = "0x0000000000000000000000000000000000000001",
            Sender = "0x0000000000000000000000000000000000000002",
            LeasingCore = "0x0000000000000000000000000000000000000003",
            Amount = "1000",
            Fee = "100000",
            Deadline = "9999999999",
            PermitSignature = new PermitSignatureDto { V = 27, R = new string('a', 64), S = new string('b', 64) }
        };

        var response = new RelayerTransactionResponseDto
        {
            Hash = "0xabc",
            Status = true,
            BlockNumber = 123,
            GasUsed = "500000",
            EffectiveGasPrice = "30000000000"
        };

        var status = new RelayerStatusDto
        {
            Configured = true,
            Network = "testnet",
            RpcConfigured = true,
            PaymasterAddress = "0x0000000000000000000000000000000000000004",
            RelayerAddress = "0x0000000000000000000000000000000000000005",
            NativeBalance = "0.1",
            HasMinimumBalance = true
        };

        Assert.Equal("payment", request.Command);
        Assert.True(response.Status);
        Assert.True(status.Configured);
    }

    [Fact]
    public void IRelayerServiceContractExists()
    {
        var serviceType = typeof(IRelayerService);

        Assert.NotNull(serviceType.GetMethod("SponsorCommitAsync"));
        Assert.NotNull(serviceType.GetMethod("SponsorPaymentAsync"));
        Assert.NotNull(serviceType.GetMethod("SponsorClaimRentAsync"));
        Assert.NotNull(serviceType.GetMethod("SponsorAsync"));
        Assert.NotNull(serviceType.GetMethod("GetStatusAsync"));
    }

    [Theory]
    [InlineData("commitFunds", new[] { "address", "address", "address", "uint256", "uint256", "uint256", "uint8", "bytes32", "bytes32" })]
    [InlineData("receivePaymentSponsored", new[] { "address", "address", "address", "uint256", "uint256", "uint256", "uint8", "bytes32", "bytes32" })]
    [InlineData("claimRent", new[] { "address", "address", "address", "uint256", "uint256", "uint8", "bytes32", "bytes32" })]
    public void PaymasterAbiContainsRelayerFunctionsWithExpectedInputs(string functionName, string[] expectedTypes)
    {
        using var document = JsonDocument.Parse(ReadPaymasterAbi());
        var function = document.RootElement.EnumerateArray().Single(item =>
            item.GetProperty("type").GetString() == "function" &&
            item.GetProperty("name").GetString() == functionName);

        var actualTypes = function.GetProperty("inputs")
            .EnumerateArray()
            .Select(input => input.GetProperty("type").GetString())
            .ToArray();

        Assert.Equal(expectedTypes, actualTypes);
    }

    [Fact]
    public void RelayerBuildsBytes32SignatureParametersAsByteArrays()
    {
        var method = typeof(RelayerService).GetMethod("BuildFunctionParameters", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        var request = new RelayerSponsorRequestDto
        {
            Command = "payment",
            Token = "0x0000000000000000000000000000000000000001",
            Sender = "0x0000000000000000000000000000000000000002",
            LeasingCore = "0x0000000000000000000000000000000000000003",
            Amount = "1000",
            Fee = "100000",
            Deadline = "9999999999",
            PermitSignature = new PermitSignatureDto
            {
                V = 27,
                R = "0x" + new string('a', 64),
                S = "0x" + new string('b', 64)
            }
        };

        var parameters = Assert.IsType<object[]>(method.Invoke(null, new object[] { "payment", request }));

        var r = Assert.IsType<byte[]>(parameters[7]);
        var s = Assert.IsType<byte[]>(parameters[8]);
        Assert.Equal(32, r.Length);
        Assert.Equal(32, s.Length);
    }

    private static string ReadPaymasterAbi()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/BricklePlatform.Infrastructure/Constants/Contracts/PaymasterAbi.json"));

        return File.ReadAllText(path);
    }
}
