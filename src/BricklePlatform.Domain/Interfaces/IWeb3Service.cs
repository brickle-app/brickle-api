using System.Numerics;
using BricklePlatform.Domain.Models;

namespace BricklePlatform.Domain.Interfaces;

public interface IWeb3Service
{
    // Provider operations
    Task<string> GetNetworkNameAsync(string? networkName = null);
    Task<int> GetBlockNumberAsync(string? networkName = null);

    // Account operations
    Task<string> GetBalanceAsync(string address, string? networkName = null);
    string CreateAccountFromPrivateKey(string privateKey);

    Task<BigInteger> EstimateGasAsync(string contractAddress, string abi, string account, string functionName, object[] parameters);

    // Contract operations
    Task<T> CallContractFunctionAsync<T>(string contractAddress, string abi, string functionName, object[] parameters, string? networkName = null);
    Task<T> CallContractFunctionDeserializingAsync<T>(string contractAddress, string abi, string functionName, object[] parameters, string? networkName = null) where T : class, new();

    // Transaction operations
    Task<string> SendTransactionAsync(string privateKey, string toAddress, decimal amountInEther, string? networkName = null);
    Task<TransactionExecuteModel> ExecuteContractFunctionAsync(string privateKey, string contractAddress, string abi, string functionName, object[] parameters, string? networkName = null, int? gasLimit = null);
    Task<string> SignTransactionAsync(string privateKey, string toAddress, decimal amountInEther, decimal? gasPriceInGwei = null, int? gasLimit = null, string? networkName = null);
}