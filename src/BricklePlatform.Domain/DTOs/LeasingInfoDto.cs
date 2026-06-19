using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace BricklePlatform.Domain.DTOs;

[FunctionOutput]
[Struct("LeasingInfo")]
public class LeasingInfoDto : IFunctionOutputDTO
{
  [Parameter("uint256", "assetValue", 1)]
  public BigInteger assetValue { get; set; }

  [Parameter("uint256", "usefulLife", 2)]
  public BigInteger usefulLife { get; set; }

  [Parameter("uint256", "termMonths", 3)]
  public BigInteger termMonths { get; set; }

  [Parameter("uint256", "leasingTokenPrice", 4)]
  public BigInteger leasingTokenPrice { get; set; }

  [Parameter("uint256", "monthlyRate", 5)]
  public BigInteger monthlyRate { get; set; }

  [Parameter("uint256", "monthlyPayment", 6)]
  public BigInteger monthlyPayment { get; set; }

  [Parameter("uint256", "managementFee", 7)]
  public BigInteger managementFee { get; set; }

  [Parameter("uint256", "insurancePct", 8)]
  public BigInteger insurancePct { get; set; }

  [Parameter("uint256", "ibrRate", 9)]
  public BigInteger ibrRate { get; set; }

  [Parameter("uint256", "riskLevel", 10)]
  public BigInteger riskLevel { get; set; }

  [Parameter("uint256", "riskRate", 11)]
  public BigInteger riskRate { get; set; }

  [Parameter("uint256", "IVA", 12)]
  public BigInteger IVA { get; set; }

  [Parameter("uint256", "reteIcaPct", 13)]
  public BigInteger reteIcaPct { get; set; }

  [Parameter("uint256", "reteFuentePct", 14)]
  public BigInteger reteFuentePct { get; set; }

  [Parameter("uint256", "finalPaymentAmount", 15)]
  public BigInteger finalPaymentAmount { get; set; }

  [Parameter("uint256", "buyerRetentionPercentage", 16)]
  public BigInteger buyerRetentionPercentage { get; set; }
}