using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Nethereum.Web3.Accounts;
using Nethereum.Util;
using System.Numerics;
using System.Reflection;

namespace BricklePlatform.Infrastructure.Services;

public class LeasingCoreService : ILeasingCoreService
{
    private readonly IWeb3Service _web3Service;
    private readonly ILogger<LeasingCoreService> _logger;

    public LeasingCoreService(IWeb3Service web3Service, ILogger<LeasingCoreService> logger)
    {
        _web3Service = web3Service;
        _logger = logger;
    }

    public async Task<string?> GetBaseTokenAsync(string leasingCoreAddress)
    {
        if (string.IsNullOrWhiteSpace(leasingCoreAddress) || leasingCoreAddress == "0x0000000000000000000000000000000000000000")
            return null;

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("BricklePlatform.Infrastructure.Constants.Contracts.LeasingCoreAbi.json");
            if (stream == null)
            {
                _logger.LogWarning("LeasingCoreAbi.json not found as embedded resource");
                return null;
            }
            using var reader = new StreamReader(stream);
            var abiJson = await reader.ReadToEndAsync();

            var baseToken = await _web3Service.CallContractFunctionAsync<string>(
                leasingCoreAddress,
                abiJson,
                "baseToken",
                Array.Empty<object>());

            return string.IsNullOrWhiteSpace(baseToken) ? null : baseToken;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read baseToken from LeasingCore {Address}", leasingCoreAddress);
            return null;
        }
    }

    public async Task<BigInteger?> GetExpectedMonthlyPaymentAsync(string leasingCoreAddress)
    {
        if (string.IsNullOrWhiteSpace(leasingCoreAddress) || leasingCoreAddress == "0x0000000000000000000000000000000000000000")
            return null;

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("BricklePlatform.Infrastructure.Constants.Contracts.LeasingCoreAbi.json");
            if (stream == null)
            {
                _logger.LogWarning("LeasingCoreAbi.json not found as embedded resource");
                return null;
            }
            using var reader = new StreamReader(stream);
            var abiJson = await reader.ReadToEndAsync();

            var finance = await _web3Service.CallContractFunctionDeserializingAsync<LeasingFinanceOutputDto>(
                leasingCoreAddress,
                abiJson,
                "leasingFinance",
                Array.Empty<object>());

            return finance?.TotalMonthlyPayment ?? null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read leasingFinance from LeasingCore {Address}", leasingCoreAddress);
            return null;
        }
    }

    public async Task<ExpectedPaymentResult?> GetExpectedPaymentAsync(string leasingCoreAddress)
    {
        if (string.IsNullOrWhiteSpace(leasingCoreAddress) || leasingCoreAddress == "0x0000000000000000000000000000000000000000")
            return null;

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("BricklePlatform.Infrastructure.Constants.Contracts.LeasingCoreAbi.json");
            if (stream == null)
            {
                _logger.LogWarning("LeasingCoreAbi.json not found as embedded resource");
                return null;
            }
            using var reader = new StreamReader(stream);
            var abiJson = await reader.ReadToEndAsync();

            var finance = await _web3Service.CallContractFunctionDeserializingAsync<LeasingFinanceOutputDto>(
                leasingCoreAddress,
                abiJson,
                "leasingFinance",
                Array.Empty<object>());
            if (finance == null)
                return null;

            var leasingInfo = await _web3Service.CallContractFunctionDeserializingAsync<LeasingInfoDto>(
                leasingCoreAddress,
                abiJson,
                "leasingInfo",
                Array.Empty<object>());
            if (leasingInfo == null)
                return null;

            var currentMonth = await _web3Service.CallContractFunctionAsync<BigInteger>(
                leasingCoreAddress,
                CurrentMonthAbi,
                "currentMonth",
                Array.Empty<object>());

            var lastPaymentMade = await _web3Service.CallContractFunctionAsync<bool>(
                leasingCoreAddress,
                abiJson,
                "lastPaymentMade",
                Array.Empty<object>());

            if (lastPaymentMade)
                return null;

            if (currentMonth < leasingInfo.termMonths)
                return new ExpectedPaymentResult(finance.TotalMonthlyPayment, IsResidualPayment: false);

            if (finance.ResidualValue > 0)
                return new ExpectedPaymentResult(finance.ResidualValue, IsResidualPayment: true);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read expected payment from LeasingCore {Address}", leasingCoreAddress);
            return null;
        }
    }

    private static readonly string CurrentMonthAbi = """[{"inputs":[],"name":"currentMonth","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"}]""";

    public async Task<BigInteger?> GetCurrentMonthAsync(string leasingCoreAddress)
    {
        if (string.IsNullOrWhiteSpace(leasingCoreAddress) || leasingCoreAddress == "0x0000000000000000000000000000000000000000")
            return null;

        try
        {
            var currentMonth = await _web3Service.CallContractFunctionAsync<BigInteger>(
                leasingCoreAddress,
                CurrentMonthAbi,
                "currentMonth",
                Array.Empty<object>());
            return currentMonth;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read currentMonth from LeasingCore {Address}", leasingCoreAddress);
            return null;
        }
    }

    public async Task<LeasingContractStateDto?> GetLeasingContractStateAsync(string leasingCoreAddress)
    {
        if (string.IsNullOrWhiteSpace(leasingCoreAddress) || leasingCoreAddress == "0x0000000000000000000000000000000000000000")
            return null;

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("BricklePlatform.Infrastructure.Constants.Contracts.LeasingCoreAbi.json");
            if (stream == null)
                return null;
            using var reader = new StreamReader(stream);
            var abiJson = await reader.ReadToEndAsync();

            var finance = await _web3Service.CallContractFunctionDeserializingAsync<LeasingFinanceOutputDto>(
                leasingCoreAddress,
                abiJson,
                "leasingFinance",
                Array.Empty<object>());
            var leasingInfo = await _web3Service.CallContractFunctionDeserializingAsync<LeasingInfoDto>(
                leasingCoreAddress,
                abiJson,
                "leasingInfo",
                Array.Empty<object>());
            if (finance == null || leasingInfo == null)
                return null;

            var currentMonth = await GetCurrentMonthAsync(leasingCoreAddress);
            var lastPaymentMade = await _web3Service.CallContractFunctionAsync<bool>(
                leasingCoreAddress,
                abiJson,
                "lastPaymentMade",
                Array.Empty<object>());

            var (leasingTokenAddr, leasingTokenSupply) = await TryGetLeasingTokenSupplyAsync(leasingCoreAddress, abiJson);

            if (lastPaymentMade)
                return new LeasingContractStateDto(0, false, currentMonth.HasValue ? (int)currentMonth.Value : 0, (int)leasingInfo.termMonths, true,
                    LeasingTokenAddress: leasingTokenAddr, LeasingTokenTotalSupply: leasingTokenSupply);

            var isResidual = currentMonth.HasValue && currentMonth.Value >= leasingInfo.termMonths && finance.ResidualValue > 0;
            var expectedAmount = isResidual ? finance.ResidualValue : finance.TotalMonthlyPayment;

            return new LeasingContractStateDto(
                expectedAmount,
                isResidual,
                currentMonth.HasValue ? (int)currentMonth.Value : 0,
                (int)leasingInfo.termMonths,
                lastPaymentMade,
                isResidual ? finance.ResidualValue : null,
                isResidual ? leasingInfo.finalPaymentAmount : null,
                leasingTokenAddr,
                leasingTokenSupply);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read contract state from LeasingCore {Address}", leasingCoreAddress);
            return null;
        }
    }

    private static readonly string Erc20BalanceOfAbi = """[{"inputs":[{"internalType":"address","name":"account","type":"address"}],"name":"balanceOf","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"}]""";

    private static readonly string Erc20TotalSupplyAbi = """[{"inputs":[],"name":"totalSupply","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"}]""";

    /// <summary>
    /// Lee leasingToken() del Core y totalSupply() del LeasingToken (participación). Independiente del saldo del token base en el Core.
    /// </summary>
    private async Task<(string? TokenAddress, BigInteger? TotalSupply)> TryGetLeasingTokenSupplyAsync(
        string leasingCoreAddress,
        string leasingCoreAbiJson)
    {
        try
        {
            var tokenAddr = await _web3Service.CallContractFunctionAsync<string>(
                leasingCoreAddress,
                leasingCoreAbiJson,
                "leasingToken",
                Array.Empty<object>());
            if (string.IsNullOrWhiteSpace(tokenAddr) ||
                string.Equals(tokenAddr, "0x0000000000000000000000000000000000000000", StringComparison.OrdinalIgnoreCase))
                return (tokenAddr, null);

            var supply = await _web3Service.CallContractFunctionAsync<BigInteger>(
                tokenAddr,
                Erc20TotalSupplyAbi,
                "totalSupply",
                Array.Empty<object>());
            return (tokenAddr, supply);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read LeasingToken totalSupply for Core {Address}", leasingCoreAddress);
            return (null, null);
        }
    }

    public async Task<BigInteger?> GetErc20BalanceAsync(string tokenAddress, string walletAddress)
    {
        if (string.IsNullOrWhiteSpace(tokenAddress) || string.IsNullOrWhiteSpace(walletAddress))
            return null;

        try
        {
            var balance = await _web3Service.CallContractFunctionAsync<BigInteger>(
                tokenAddress,
                Erc20BalanceOfAbi,
                "balanceOf",
                new object[] { walletAddress });

            return balance;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read balanceOf from token {Token} for wallet {Wallet}", tokenAddress, walletAddress);
            return null;
        }
    }

    public async Task<string> SendMakeLastLeasingPaymentAsync(string privateKey, string leasingCoreAddress, string clientAddress, BigInteger residualValueWei, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(privateKey))
            throw new ArgumentException("Private key is required", nameof(privateKey));
        if (string.IsNullOrWhiteSpace(leasingCoreAddress) || leasingCoreAddress == "0x0000000000000000000000000000000000000000")
            throw new ArgumentException("Invalid LeasingCore address", nameof(leasingCoreAddress));
        if (string.IsNullOrWhiteSpace(clientAddress))
            throw new ArgumentException("Client address is required", nameof(clientAddress));
        if (residualValueWei <= 0)
            throw new ArgumentException("Residual value must be positive", nameof(residualValueWei));

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("BricklePlatform.Infrastructure.Constants.Contracts.LeasingCoreAbi.json");
        if (stream == null)
            throw new InvalidOperationException("LeasingCoreAbi.json not found as embedded resource");

        using var reader = new StreamReader(stream);
        var abiJson = await reader.ReadToEndAsync();

        var signer = new Account(privateKey);
        var callParams = new object[] { clientAddress, residualValueWei };
        try
        {
            await _web3Service.EstimateGasAsync(
                leasingCoreAddress,
                abiJson,
                signer.Address,
                "makeLastLeasingPayment",
                callParams);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "makeLastLeasingPayment: estimateGas falló (la tx revertiría). Core={Core}, residualWei={Residual}",
                leasingCoreAddress, residualValueWei);
            throw new InvalidOperationException(
                "makeLastLeasingPayment revertiría en cadena. Revisa: (1) saldo del LeasingCore en el token base ≥ residual + finalPaymentAmount, " +
                "(2) todas las cuotas mensuales completadas (estado Payed), (3) lastPaymentMade aún false, " +
                "(4) el amount coincide exactamente con leasingFinance.residualValue, " +
                "(5) totalSupply del LeasingToken > 0 (si es 0: «No leasing token supply»). Mensaje RPC: " + ex.Message,
                ex);
        }

        const int gasLimitLastPayment = 1_500_000;
        var result = await _web3Service.ExecuteContractFunctionAsync(
            privateKey,
            leasingCoreAddress,
            abiJson,
            "makeLastLeasingPayment",
            callParams,
            null,
            gasLimitLastPayment);

        var receipt = result.Receipt;
        if (receipt.Status == null || receipt.Status.Value == 0)
            throw new InvalidOperationException(
                "makeLastLeasingPayment falló (receipt status 0). Si el estimateGas pasó, puede ser límite de gas bajo o estado de cadena distinto al simulado.");

        return receipt.TransactionHash ?? string.Empty;
    }

    public async Task<bool> ShouldOmitFromActiveInvestmentsListAsync(
        string leasingCoreAddress,
        string investorWalletAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(leasingCoreAddress) ||
            leasingCoreAddress.Equals("0x0000000000000000000000000000000000000000", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(investorWalletAddress))
            return false;

        if (!AddressUtil.Current.IsValidEthereumAddressHexFormat(leasingCoreAddress) ||
            !AddressUtil.Current.IsValidEthereumAddressHexFormat(investorWalletAddress))
        {
            _logger.LogWarning(
                "ShouldOmitFromActiveInvestmentsListAsync: direcciones inválidas Core={Core}, Wallet={Wallet}",
                leasingCoreAddress,
                investorWalletAddress);
            return false;
        }

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("BricklePlatform.Infrastructure.Constants.Contracts.LeasingCoreAbi.json");
            if (stream == null)
                return false;

            using var reader = new StreamReader(stream);
            var abiJson = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            var lastPaymentMade = await _web3Service.CallContractFunctionAsync<bool>(
                leasingCoreAddress,
                abiJson,
                "lastPaymentMade",
                Array.Empty<object>()).ConfigureAwait(false);

            if (!lastPaymentMade)
                return false;

            var claimable = await _web3Service.CallContractFunctionAsync<BigInteger>(
                leasingCoreAddress,
                abiJson,
                "getClaimableEarnings",
                new object[] { investorWalletAddress }).ConfigureAwait(false);

            return claimable == BigInteger.Zero;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "ShouldOmitFromActiveInvestmentsListAsync: lectura fallida para Core {Core}, wallet {Wallet}",
                leasingCoreAddress, investorWalletAddress);
            return false;
        }
    }
}
