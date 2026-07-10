using System.Numerics;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.DTOs.Relayer;
using BricklePlatform.Infrastructure.Services;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Nethereum.Util;
using Nethereum.Web3;
using Xunit;

namespace BricklePlatform.Test.Integration;

public class AmoyRelayerIntegrationTests
{
    [Fact]
    public async Task AmoySmokeReadsFactoryBaseTokenAndRelayerStatus()
    {
        if (!AmoyRelayerTestConfig.TryLoad(out var config, out var skipReason))
        {
            return;
        }

        Assert.NotNull(config);
        Assert.True(AddressUtil.Current.IsValidEthereumAddressHexFormat(AmoyRelayerTestConfig.MockErc20));
        Assert.True(AddressUtil.Current.IsValidEthereumAddressHexFormat(AmoyRelayerTestConfig.Paymaster));
        Assert.True(AddressUtil.Current.IsValidEthereumAddressHexFormat(AmoyRelayerTestConfig.ThresholdFactory));
        Assert.True(AddressUtil.Current.IsValidEthereumAddressHexFormat(AmoyRelayerTestConfig.BrickleNft));

        var web3 = new Web3(config.RpcUrl);
        var balance = await web3.Eth.GetBalance.SendRequestAsync(config.RelayerAddress);
        Assert.True(balance.Value > BigInteger.Zero, $"Relayer {config.RelayerAddress} needs Amoy MATIC. Skip reason if disabled: {skipReason}");

        var thresholdAbi = await File.ReadAllTextAsync(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/BricklePlatform.Infrastructure/Constants/Contracts/ThresholdFactoryAbi.json")));
        var threshold = web3.Eth.GetContract(thresholdAbi, AmoyRelayerTestConfig.ThresholdFactory);
        var baseToken = await threshold.GetFunction("baseToken").CallAsync<string>();
        Assert.Equal(AmoyRelayerTestConfig.MockErc20, baseToken, ignoreCase: true);

        var service = new RelayerService(Options.Create(CreateSettings(config)), NullLogger<RelayerService>.Instance);
        var status = await service.GetStatusAsync();
        Assert.True(status.Configured, status.ErrorMessage);
        Assert.Equal(config.RelayerAddress, status.RelayerAddress, ignoreCase: true);
    }

    [Fact]
    public async Task AmoyEndToEndCreatesCampaignAndSponsorsCommitAndPayment()
    {
        if (!AmoyRelayerTestConfig.TryLoad(out var config, out _))
        {
            return;
        }

        Assert.NotNull(config);
        var settings = Options.Create(CreateSettings(config));
        var web3Service = new Web3Service(settings, NullLogger<Web3Service>.Instance);
        var factoryService = new ThresholdFactoryService(web3Service, settings);
        var relayerService = new RelayerService(settings, NullLogger<RelayerService>.Instance);
        var permitSigner = new Eip2612PermitSigner(config.RpcUrl);

        await AssertRequiredBalancesAsync(config);

        var deadline = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds();
        var campaign = await factoryService.CreateCampaign(CreateCampaignInfo(config, deadline), CreateLeasingInfo());
        Assert.True(AddressUtil.Current.IsValidEthereumAddressHexFormat(campaign.ContractAddress));

        var commitAmount = new BigInteger(1_000_000);
        var fee = new BigInteger(100_000);
        var commitPermit = await permitSigner.SignPermitAsync(config.UserPrivateKey, AmoyRelayerTestConfig.MockErc20, AmoyRelayerTestConfig.Paymaster, commitAmount + fee, deadline);
        var commit = await relayerService.SponsorAsync(new RelayerSponsorRequestDto
        {
            Command = "commit",
            Token = AmoyRelayerTestConfig.MockErc20,
            Sender = config.UserAddress,
            Campaign = campaign.ContractAddress,
            Amount = commitAmount.ToString(),
            Fee = fee.ToString(),
            Deadline = deadline.ToString(),
            PermitSignature = commitPermit
        });

        Assert.True(commit.Status, commit.ErrorMessage);
        Assert.StartsWith("0x", commit.Hash);
        Assert.NotNull(commit.BlockNumber);

        var finalized = await factoryService.FinalizeCampaign(campaign.ContractAddress);
        Assert.True(AddressUtil.Current.IsValidEthereumAddressHexFormat(finalized.LeasingCoreAddress));
        Assert.Equal(AmoyRelayerTestConfig.MockErc20, finalized.TokenAddress, ignoreCase: true);

        var paymentAmount = new BigInteger(1_000_000);
        var paymentDeadline = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds();
        var paymentPermit = await permitSigner.SignPermitAsync(config.UserPrivateKey, AmoyRelayerTestConfig.MockErc20, AmoyRelayerTestConfig.Paymaster, paymentAmount + fee, paymentDeadline);
        var payment = await relayerService.SponsorAsync(new RelayerSponsorRequestDto
        {
            Command = "payment",
            Token = AmoyRelayerTestConfig.MockErc20,
            Sender = config.UserAddress,
            LeasingCore = finalized.LeasingCoreAddress,
            Amount = paymentAmount.ToString(),
            Fee = fee.ToString(),
            Deadline = paymentDeadline.ToString(),
            PermitSignature = paymentPermit
        });

        Assert.True(payment.Status, payment.ErrorMessage);
        Assert.StartsWith("0x", payment.Hash);
        Assert.NotNull(payment.BlockNumber);
    }

    private static InfrastructureSettings CreateSettings(AmoyRelayerTestConfig config) => new()
    {
        Web3Settings = new Web3Settings
        {
            RpcUrl = config.RpcUrl,
            Network = "amoy",
            WalletPrivateKey = config.WalletPrivateKey,
            RelayerPrivateKey = config.RelayerPrivateKey,
            RelayerMinNativeBalance = 0.001m,
            BASE_TOKEN = AmoyRelayerTestConfig.MockErc20,
            PAYMASTER = AmoyRelayerTestConfig.Paymaster,
            THRESHOLD_FACTORY = AmoyRelayerTestConfig.ThresholdFactory,
            BRICKLE_NFT = AmoyRelayerTestConfig.BrickleNft
        }
    };

    private static CampaignInfoDto CreateCampaignInfo(AmoyRelayerTestConfig config, long deadline) => new()
    {
        minCap = 1_000_000,
        maxCap = 1_000_000,
        totalLeasingTokens = 1,
        tokenPrice = 1_000_000,
        deadline = deadline,
        baseToken = AmoyRelayerTestConfig.MockErc20,
        brickleAddress = config.WalletAddress
    };

    private static LeasingInfoDto CreateLeasingInfo() => new()
    {
        assetValue = 1_000_000,
        usefulLife = 12,
        termMonths = 1,
        leasingTokenPrice = 1_000_000,
        monthlyRate = 0,
        monthlyPayment = 1_000_000,
        managementFee = 0,
        insurancePct = 0,
        ibrRate = 0,
        riskLevel = 1,
        riskRate = 0,
        IVA = 0,
        reteIcaPct = 0,
        reteFuentePct = 0,
        finalPaymentAmount = 0,
        buyerRetentionPercentage = 0
    };

    private static async Task AssertRequiredBalancesAsync(AmoyRelayerTestConfig config)
    {
        var web3 = new Web3(config.RpcUrl);
        var walletMatic = await web3.Eth.GetBalance.SendRequestAsync(config.WalletAddress);
        var relayerMatic = await web3.Eth.GetBalance.SendRequestAsync(config.RelayerAddress);
        Assert.True(walletMatic.Value > BigInteger.Zero, $"Operation wallet {config.WalletAddress} needs Amoy MATIC for ThresholdFactory transactions.");
        Assert.True(relayerMatic.Value > BigInteger.Zero, $"Relayer wallet {config.RelayerAddress} needs Amoy MATIC for Paymaster transactions.");

        const string erc20Abi = """
        [{ "type": "function", "name": "balanceOf", "inputs": [{ "name": "account", "type": "address" }], "outputs": [{ "name": "", "type": "uint256" }], "stateMutability": "view" }]
        """;
        var token = web3.Eth.GetContract(erc20Abi, AmoyRelayerTestConfig.MockErc20);
        var userTokenBalance = await token.GetFunction("balanceOf").CallAsync<BigInteger>(config.UserAddress);
        Assert.True(userTokenBalance >= new BigInteger(2_200_000), $"Test user {config.UserAddress} needs at least 2200000 MockERC20 units for commit/payment plus fees.");
    }
}
