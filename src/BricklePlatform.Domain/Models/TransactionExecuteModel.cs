using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;

namespace BricklePlatform.Domain.Models;

public class TransactionExecuteModel
{
  public TransactionReceipt Receipt { get; set; }
  public Contract Contract { get; set; }
}