using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Domain.Models;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;

namespace BricklePlatform.Infrastructure.Services;

public class Web3Service : IWeb3Service
{
    private readonly IOptions<InfrastructureSettings> _settings;
    private readonly ILogger<Web3Service> _logger;
    private readonly Dictionary<string, Web3> _web3Instances;

    public Web3Service(IOptions<InfrastructureSettings> settings, ILogger<Web3Service> logger)
    {
        _settings = settings;
        _logger = logger;
        _web3Instances = new Dictionary<string, Web3>();
    }

    #region Provider Operations

    private Web3 GetWeb3Instance(string? networkName = null)
    {
        var network = networkName ?? _settings.Value.Web3Settings.Network;

        if (_web3Instances.TryGetValue(network, out var existingInstance))
        {
            return existingInstance;
        }

        var url = _settings.Value.Web3Settings.RpcUrl;

        if (string.IsNullOrEmpty(url))
        {
            throw new InvalidOperationException($"No URL configured for network: {network}");
        }


        var web3Instance = new Web3(url);
        web3Instance.TransactionManager.UseLegacyAsDefault = true;
        _web3Instances[network] = web3Instance;

        _logger.LogInformation("Created Web3 instance for network: {Network} with URL: {Url}", network, url);


        return web3Instance;
    }

    public async Task<BigInteger> EstimateGasAsync(string contractAddress, string abi, string account, string functionName, object[] parameters)
    {
        try
        {
            var web3 = GetWeb3Instance();
            var contract = web3.Eth.GetContract(abi, contractAddress);
            var function = contract.GetFunction(functionName);

            var gas = await function.EstimateGasAsync(
                from: account,
                gas: null,
                value: null,
                functionInput: parameters
            );
            return gas.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating gas for contract: {ContractAddress}", contractAddress);
            throw;
        }

    }

    public async Task<string> GetNetworkNameAsync(string? networkName = null)
    {
        try
        {
            var web3 = GetWeb3Instance(networkName);
            var networkVersion = await web3.Net.Version.SendRequestAsync();
            return $"Network ID: {networkVersion}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting network name for network: {NetworkName}", networkName);
            throw;
        }
    }

    public async Task<int> GetBlockNumberAsync(string? networkName = null)
    {
        try
        {
            var web3 = GetWeb3Instance(networkName);
            var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            return (int)blockNumber.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting block number for network: {NetworkName}", networkName);
            throw;
        }
    }

    #endregion

    #region Account Operations

    public async Task<string> GetBalanceAsync(string address, string? networkName = null)
    {
        try
        {
            var web3 = GetWeb3Instance(networkName);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(address);
            var etherAmount = Web3.Convert.FromWei(balance.Value);
            return etherAmount.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance for address: {Address}", address);
            throw;
        }
    }

    public string CreateAccountFromPrivateKey(string privateKey)
    {
        try
        {
            var account = new Account(privateKey);
            return account.Address;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account from private key");
            throw;
        }
    }

    #endregion

    #region Contract Operations

    public async Task<T> CallContractFunctionAsync<T>(string contractAddress, string abi, string functionName, object[] parameters, string? networkName = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contractAddress) ||
                !AddressUtil.Current.IsValidEthereumAddressHexFormat(contractAddress))
            {
                throw new ArgumentException($"Invalid contract address: {contractAddress}", nameof(contractAddress));
            }

            var web3 = GetWeb3Instance(networkName);
            var contract = web3.Eth.GetContract(abi, contractAddress);
            var function = contract.GetFunction(functionName);

            var result = await function.CallAsync<T>(parameters);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling contract function: {FunctionName} on contract: {ContractAddress}", functionName, contractAddress);
            throw;
        }
    }

    public async Task<T> CallContractFunctionDeserializingAsync<T>(string contractAddress, string abi, string functionName, object[] parameters, string? networkName = null) where T : class, new()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contractAddress) ||
                !AddressUtil.Current.IsValidEthereumAddressHexFormat(contractAddress))
            {
                throw new ArgumentException($"Invalid contract address: {contractAddress}", nameof(contractAddress));
            }

            var web3 = GetWeb3Instance(networkName);
            var contract = web3.Eth.GetContract(abi, contractAddress);
            var function = contract.GetFunction(functionName);

            var result = await function.CallDeserializingToObjectAsync<T>(parameters);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling contract function: {FunctionName} on contract: {ContractAddress}", functionName, contractAddress);
            throw;
        }
    }

    #endregion

    #region Transaction Operations

    public async Task<string> SendTransactionAsync(string privateKey, string toAddress, decimal amountInEther, string? networkName = null)
    {
        try
        {
            var account = new Account(privateKey);
            var web3 = new Web3(account, GetWeb3Instance(networkName).Client);

            var amountInWei = Web3.Convert.ToWei(amountInEther);
            var transactionHash = await web3.Eth.GetEtherTransferService()
                .TransferEtherAsync(toAddress, amountInEther);

            _logger.LogInformation("Transaction sent: {TransactionHash} from {From} to {To} amount: {Amount} ETH",
                transactionHash, account.Address, toAddress, amountInEther);

            return transactionHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending transaction to: {ToAddress} amount: {Amount}", toAddress, amountInEther);
            throw;
        }
    }

    public async Task<TransactionExecuteModel> ExecuteContractFunctionAsync(string privateKey, string contractAddress, string abi, string functionName, object[] parameters, string? networkName = null, int? gasLimit = null)
    {
        try
        {
            var account = new Account(privateKey);
            var web3 = new Web3(account, GetWeb3Instance(networkName).Client);

            var contract = web3.Eth.GetContract(abi, contractAddress);
            var function = contract.GetFunction(functionName);
            // createCampaign (new Campaign + initialize) consume ~2.95M gas; usar margen para evitar revert por gas.
            var gas = gasLimit ?? 3_000_000;
            var receipt = await function.SendTransactionAndWaitForReceiptAsync(account.Address, new HexBigInteger(gas), null, null, parameters);

            _logger.LogInformation("Contract function executed: {FunctionName} on contract: {ContractAddress} tx: {TransactionHash}",
                functionName, contractAddress, receipt.TransactionHash);

            return new TransactionExecuteModel
            {
                Receipt = receipt,
                Contract = contract
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing contract function: {FunctionName} on contract: {ContractAddress}", functionName, contractAddress);
            throw;
        }
    }

    public async Task<string> SignTransactionAsync(string privateKey, string toAddress, decimal amountInEther, decimal? gasPriceInGwei = null, int? gasLimit = null, string? networkName = null)
    {
        try
        {
            var account = new Account(privateKey);
            var web3 = new Web3(account, GetWeb3Instance(networkName).Client);

            var amountInWei = Web3.Convert.ToWei(amountInEther);

            var transactionInput = new TransactionInput
            {
                From = account.Address,
                To = toAddress,
                Value = new HexBigInteger(amountInWei),
                Gas = gasLimit != null ? new HexBigInteger(gasLimit.Value) : null,
                GasPrice = gasPriceInGwei != null ? new HexBigInteger(Web3.Convert.ToWei(gasPriceInGwei.Value, UnitConversion.EthUnit.Gwei)) : null
            };

            // Estimate gas if not provided
            if (gasLimit == null)
            {
                var gasEstimate = await web3.Eth.TransactionManager.EstimateGasAsync(transactionInput);
                transactionInput.Gas = gasEstimate;
            }

            // Get gas price if not provided
            if (gasPriceInGwei == null)
            {
                var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();
                transactionInput.GasPrice = gasPrice;
            }

            var signedTransaction = await web3.Eth.TransactionManager.SignTransactionAsync(transactionInput);

            _logger.LogInformation("Transaction signed for: {ToAddress} amount: {Amount} ETH", toAddress, amountInEther);

            return signedTransaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing transaction to: {ToAddress} amount: {Amount}", toAddress, amountInEther);
            throw;
        }
    }

    #endregion
}