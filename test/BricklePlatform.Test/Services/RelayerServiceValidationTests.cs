using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.DTOs.Relayer;
using BricklePlatform.Domain.Models;
using BricklePlatform.Infrastructure.Services;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BricklePlatform.Test.Services;

public class RelayerServiceValidationTests
{
    [Fact]
    public async Task SponsorAsyncReturnsValidationErrorForInvalidCommand()
    {
        var service = CreateService();

        var result = await service.SponsorAsync(new RelayerSponsorRequestDto { Command = "bad" });

        Assert.False(result.Status);
        Assert.Contains("Invalid command", result.ErrorMessage);
        Assert.Equal(string.Empty, result.Hash);
    }

    [Fact]
    public async Task SponsorAsyncValidatesSignatureV()
    {
        var service = CreateService();
        var request = ValidPaymentRequest();
        request.PermitSignature.V = 29;

        var result = await service.SponsorAsync(request);

        Assert.False(result.Status);
        Assert.Contains("Invalid v value", result.ErrorMessage);
    }

    [Fact]
    public async Task SponsorAsyncValidatesBytes32SignatureParts()
    {
        var service = CreateService();
        var request = ValidPaymentRequest();
        request.PermitSignature.R = "0x1234";

        var result = await service.SponsorAsync(request);

        Assert.False(result.Status);
        Assert.Contains("Invalid r or s", result.ErrorMessage);
    }

    [Theory]
    [InlineData("Token", "bad", "Invalid token address")]
    [InlineData("Sender", "bad", "Invalid sender address")]
    [InlineData("LeasingCore", "bad", "Invalid leasingCore address")]
    public async Task SponsorAsyncValidatesPaymentAddresses(string propertyName, string value, string expectedError)
    {
        var service = CreateService();
        var request = ValidPaymentRequest();
        typeof(RelayerSponsorRequestDto).GetProperty(propertyName)!.SetValue(request, value);

        var result = await service.SponsorAsync(request);

        Assert.False(result.Status);
        Assert.Contains(expectedError, result.ErrorMessage);
    }

    [Theory]
    [InlineData("Amount", "-1", "Invalid amount value")]
    [InlineData("Fee", "-1", "Invalid fee value")]
    [InlineData("Deadline", "not-a-number", "Invalid deadline value")]
    public async Task SponsorAsyncValidatesUnsignedNumericValues(string propertyName, string value, string expectedError)
    {
        var service = CreateService();
        var request = ValidPaymentRequest();
        typeof(RelayerSponsorRequestDto).GetProperty(propertyName)!.SetValue(request, value);

        var result = await service.SponsorAsync(request);

        Assert.False(result.Status);
        Assert.Contains(expectedError, result.ErrorMessage);
    }

    [Fact]
    public async Task SponsorAsyncValidatesCommitSpecificFields()
    {
        var service = CreateService();
        var request = ValidCommitRequest();
        request.Campaign = "bad";

        var result = await service.SponsorAsync(request);

        Assert.False(result.Status);
        Assert.Contains("Invalid campaign address", result.ErrorMessage);
    }

    [Fact]
    public async Task SponsorAsyncValidatesClaimRentSpecificFields()
    {
        var service = CreateService();
        var request = ValidClaimRentRequest();
        request.Receiver = "bad";

        var result = await service.SponsorAsync(request);

        Assert.False(result.Status);
        Assert.Contains("Invalid receiver address", result.ErrorMessage);
    }

    [Fact]
    public async Task GetStatusDoesNotReturnSecretsWhenRelayerPrivateKeyMissing()
    {
        var service = CreateService(relayerPrivateKey: string.Empty);

        var status = await service.GetStatusAsync();

        Assert.False(status.Configured);
        Assert.True(status.RpcConfigured);
        Assert.Equal(string.Empty, status.RelayerAddress);
        Assert.DoesNotContain("private", status.NativeBalance, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SponsorAsyncReturnsConfigurationErrorWhenRelayerKeyMissing()
    {
        var service = CreateService(relayerPrivateKey: string.Empty);

        var result = await service.SponsorAsync(ValidPaymentRequest());

        Assert.False(result.Status);
        Assert.Contains("RelayerPrivateKey", result.ErrorMessage);
    }

    private static RelayerService CreateService(string relayerPrivateKey = "0x0123456789012345678901234567890123456789012345678901234567890123")
    {
        var settings = Options.Create(new InfrastructureSettings
        {
            Web3Settings = new Web3Settings
            {
                RpcUrl = "https://example.invalid",
                Network = "testnet",
                PAYMASTER = "0x0000000000000000000000000000000000000004",
                RelayerPrivateKey = relayerPrivateKey,
                RelayerMinNativeBalance = 0.01m
            }
        });

        return new RelayerService(settings, NullLogger<RelayerService>.Instance);
    }

    private static RelayerSponsorRequestDto ValidPaymentRequest() => new()
    {
        Command = "payment",
        Token = "0x0000000000000000000000000000000000000001",
        Sender = "0x0000000000000000000000000000000000000002",
        LeasingCore = "0x0000000000000000000000000000000000000003",
        Amount = "1000",
        Fee = "100000",
        Deadline = "9999999999",
        PermitSignature = ValidPermitSignature()
    };

    private static RelayerSponsorRequestDto ValidCommitRequest() => new()
    {
        Command = "commit",
        Token = "0x0000000000000000000000000000000000000001",
        Sender = "0x0000000000000000000000000000000000000002",
        Campaign = "0x0000000000000000000000000000000000000003",
        Amount = "1000",
        Fee = "100000",
        Deadline = "9999999999",
        PermitSignature = ValidPermitSignature()
    };

    private static RelayerSponsorRequestDto ValidClaimRentRequest() => new()
    {
        Command = "claimrent",
        Token = "0x0000000000000000000000000000000000000001",
        LeasingCore = "0x0000000000000000000000000000000000000003",
        Receiver = "0x0000000000000000000000000000000000000004",
        Fee = "100000",
        Deadline = "9999999999",
        PermitSignature = ValidPermitSignature()
    };

    private static PermitSignatureDto ValidPermitSignature() => new()
    {
        V = 27,
        R = "0x" + new string('a', 64),
        S = "0x" + new string('b', 64)
    };
}
