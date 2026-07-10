using System.Numerics;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.DTOs.Relayer;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Domain.Models;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace BricklePlatform.Infrastructure.Services;

public class RelayerService : IRelayerService
{
    private const int DefaultGasLimit = 500000;
    private readonly IOptions<InfrastructureSettings> _settings;
    private readonly ILogger<RelayerService> _logger;

    public RelayerService(IOptions<InfrastructureSettings> settings, ILogger<RelayerService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public Task<RelayerTransactionResponseDto> SponsorCommitAsync(CommitFundsDto commitFundsDto, int deadline, PermitSignatureDto permitSignature, CancellationToken cancellationToken = default)
    {
        return SponsorAsync(new RelayerSponsorRequestDto
        {
            Command = "commit",
            Token = commitFundsDto.Token,
            Sender = commitFundsDto.Sender,
            Campaign = commitFundsDto.Campaign,
            Amount = commitFundsDto.Amount,
            Fee = commitFundsDto.Fee,
            Deadline = deadline.ToString(),
            PermitSignature = permitSignature
        }, cancellationToken);
    }

    public Task<RelayerTransactionResponseDto> SponsorPaymentAsync(PaymentDto paymentDto, string walletAddress, string leasingContractAddress, string tokenAddress, CancellationToken cancellationToken = default)
    {
        return SponsorAsync(new RelayerSponsorRequestDto
        {
            Command = "payment",
            Token = tokenAddress,
            Sender = walletAddress,
            LeasingCore = leasingContractAddress,
            Amount = paymentDto.PaymentAmount,
            Fee = "100000",
            Deadline = paymentDto.Deadline.ToString(),
            PermitSignature = paymentDto.PermitSignature
        }, cancellationToken);
    }

    public Task<RelayerTransactionResponseDto> SponsorClaimRentAsync(string token, string leasingCore, string receiver, int fee, int deadline, PermitSignatureDto permitSignature, CancellationToken cancellationToken = default)
    {
        return SponsorAsync(new RelayerSponsorRequestDto
        {
            Command = "claimrent",
            Token = token,
            LeasingCore = leasingCore,
            Receiver = receiver,
            Fee = fee.ToString(),
            Deadline = deadline.ToString(),
            PermitSignature = permitSignature
        }, cancellationToken);
    }

    public async Task<RelayerTransactionResponseDto> SponsorAsync(RelayerSponsorRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateRequest(request);
        if (validationError != null)
        {
            return Failure(validationError);
        }

        return await ExecuteSponsorAsync(request, cancellationToken);
    }

    public async Task<RelayerStatusDto> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var web3Settings = _settings.Value.Web3Settings;
        var rpcConfigured = !string.IsNullOrWhiteSpace(web3Settings.RpcUrl);
        var paymasterValid = IsAddress(web3Settings.PAYMASTER);
        var keyConfigured = !string.IsNullOrWhiteSpace(web3Settings.RelayerPrivateKey);

        var status = new RelayerStatusDto
        {
            Configured = rpcConfigured && paymasterValid && keyConfigured,
            Network = web3Settings.Network,
            RpcConfigured = rpcConfigured,
            PaymasterAddress = web3Settings.PAYMASTER ?? string.Empty,
            NativeBalance = string.Empty,
            HasMinimumBalance = false
        };

        if (!keyConfigured)
        {
            status.ErrorMessage = "RelayerPrivateKey is not configured";
            return status;
        }

        try
        {
            var account = new Account(web3Settings.RelayerPrivateKey);
            status.RelayerAddress = account.Address;

            if (!rpcConfigured)
            {
                status.Configured = false;
                status.ErrorMessage = "RpcUrl is not configured";
                return status;
            }

            var web3 = new Web3(web3Settings.RpcUrl);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address).ConfigureAwait(false);
            var balanceEther = Web3.Convert.FromWei(balance.Value);
            status.NativeBalance = balanceEther.ToString();
            status.HasMinimumBalance = balanceEther >= web3Settings.RelayerMinNativeBalance;
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to read relayer status");
            status.Configured = false;
            status.ErrorMessage = ex.Message;
            return status;
        }
    }

    private static string? ValidateRequest(RelayerSponsorRequestDto request)
    {
        var command = request.Command?.Trim().ToLowerInvariant();
        if (command is not ("commit" or "payment" or "claimrent"))
            return "Invalid command. Supported commands: 'commit', 'payment', 'claimrent'";

        if (!ValidateSignature(request.PermitSignature, out var signatureError))
            return signatureError;

        if (!IsAddress(request.Token)) return "Invalid token address";
        if (!IsUnsignedInteger(request.Fee)) return "Invalid fee value";
        if (!IsUnsignedInteger(request.Deadline)) return "Invalid deadline value";

        return command switch
        {
            "commit" => ValidateCommit(request),
            "payment" => ValidatePayment(request),
            "claimrent" => ValidateClaimRent(request),
            _ => "Invalid command"
        };
    }

    private static string? ValidateCommit(RelayerSponsorRequestDto request)
    {
        if (!IsAddress(request.Sender)) return "Invalid sender address";
        if (!IsAddress(request.Campaign)) return "Invalid campaign address";
        if (!IsUnsignedInteger(request.Amount)) return "Invalid amount value";
        return null;
    }

    private static string? ValidatePayment(RelayerSponsorRequestDto request)
    {
        if (!IsAddress(request.Sender)) return "Invalid sender address";
        if (!IsAddress(request.LeasingCore)) return "Invalid leasingCore address";
        if (!IsUnsignedInteger(request.Amount)) return "Invalid amount value";
        return null;
    }

    private static string? ValidateClaimRent(RelayerSponsorRequestDto request)
    {
        if (!IsAddress(request.LeasingCore)) return "Invalid leasingCore address";
        if (!IsAddress(request.Receiver)) return "Invalid receiver address";
        return null;
    }

    private static bool ValidateSignature(PermitSignatureDto? signature, out string? error)
    {
        error = null;
        if (signature == null)
        {
            error = "Missing permit signature";
            return false;
        }

        if (signature.V != 27 && signature.V != 28)
        {
            error = "Invalid v value in signature";
            return false;
        }

        if (!IsBytes32(signature.R) || !IsBytes32(signature.S))
        {
            error = "Invalid r or s values in signature";
            return false;
        }

        return true;
    }

    private static bool IsAddress(string? value) => !string.IsNullOrWhiteSpace(value) && AddressUtil.Current.IsValidEthereumAddressHexFormat(value);

    private static bool IsUnsignedInteger(string? value) => BigInteger.TryParse(value, out var parsed) && parsed >= BigInteger.Zero;

    private static bool IsBytes32(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var normalized = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? value[2..] : value;
        return normalized.Length == 64 && normalized.All(Uri.IsHexDigit);
    }

    private static RelayerTransactionResponseDto Failure(string message) => new()
    {
        Hash = string.Empty,
        Status = false,
        ErrorMessage = message
    };

    private Task<RelayerTransactionResponseDto> ExecuteSponsorAsync(RelayerSponsorRequestDto request, CancellationToken cancellationToken)
    {
        return ExecuteSponsorInternalAsync(request, cancellationToken);
    }

    private async Task<RelayerTransactionResponseDto> ExecuteSponsorInternalAsync(RelayerSponsorRequestDto request, CancellationToken cancellationToken)
    {
        var web3Settings = _settings.Value.Web3Settings;
        var configError = ValidateConfiguration(web3Settings);
        if (configError != null) return Failure(configError);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var account = new Account(web3Settings.RelayerPrivateKey);
            var web3 = new Web3(account, web3Settings.RpcUrl);
            web3.TransactionManager.UseLegacyAsDefault = true;

            var abi = LoadPaymasterAbi();
            var contract = web3.Eth.GetContract(abi, web3Settings.PAYMASTER);
            var command = request.Command.Trim().ToLowerInvariant();
            var functionName = command switch
            {
                "commit" => "commitFunds",
                "payment" => "receivePaymentSponsored",
                "claimrent" => "claimRent",
                _ => throw new InvalidOperationException($"Unsupported relayer command: {request.Command}")
            };

            var function = contract.GetFunction(functionName);
            var parameters = BuildFunctionParameters(command, request);
            var receipt = await function.SendTransactionAndWaitForReceiptAsync(
                account.Address,
                new HexBigInteger(DefaultGasLimit),
                null,
                null,
                parameters).ConfigureAwait(false);

            var succeeded = receipt.Status?.Value == BigInteger.One;
            return new RelayerTransactionResponseDto
            {
                Hash = receipt.TransactionHash,
                Status = succeeded,
                BlockNumber = receipt.BlockNumber?.Value == null ? null : (long)receipt.BlockNumber.Value,
                GasUsed = receipt.GasUsed?.Value.ToString(),
                EffectiveGasPrice = receipt.EffectiveGasPrice?.Value.ToString(),
                ErrorMessage = succeeded ? null : "Transaction receipt status is failed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Relayer transaction failed for command {Command}", request.Command);
            return Failure($"Relayer transaction failed: {ex.Message}");
        }
    }

    private static string? ValidateConfiguration(Web3Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.RelayerPrivateKey)) return "RelayerPrivateKey is not configured";
        if (string.IsNullOrWhiteSpace(settings.RpcUrl)) return "RpcUrl is not configured";
        if (!IsAddress(settings.PAYMASTER)) return "PAYMASTER is not configured or invalid";
        return null;
    }

    private static object[] BuildFunctionParameters(string command, RelayerSponsorRequestDto request)
    {
        var v = Convert.ToByte(request.PermitSignature.V);
        var r = ToBytes32(request.PermitSignature.R);
        var s = ToBytes32(request.PermitSignature.S);
        return command switch
        {
            "commit" => new object[] { request.Token, request.Sender, request.Campaign, BigInteger.Parse(request.Amount), BigInteger.Parse(request.Fee), BigInteger.Parse(request.Deadline), v, r, s },
            "payment" => new object[] { request.Token, request.Sender, request.LeasingCore, BigInteger.Parse(request.Amount), BigInteger.Parse(request.Fee), BigInteger.Parse(request.Deadline), v, r, s },
            "claimrent" => new object[] { request.Token, request.LeasingCore, request.Receiver, BigInteger.Parse(request.Fee), BigInteger.Parse(request.Deadline), v, r, s },
            _ => throw new InvalidOperationException($"Unsupported relayer command: {command}")
        };
    }

    private static byte[] ToBytes32(string value)
    {
        var normalized = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? value[2..] : value;
        return Convert.FromHexString(normalized);
    }

    private static string LoadPaymasterAbi()
    {
        var assembly = typeof(RelayerService).Assembly;
        var resourceName = assembly.GetManifestResourceNames().Single(name => name.EndsWith("PaymasterAbi.json", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException("Paymaster ABI resource not found");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
