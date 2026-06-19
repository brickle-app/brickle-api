using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace BricklePlatform.Domain.DTOs;

[FunctionOutput]
public class LeasingFinanceOutputDto : IFunctionOutputDTO
{
    [Parameter("uint256", "residualValue", 1)]
    public BigInteger ResidualValue { get; set; }

    [Parameter("uint256", "annualInsurance", 2)]
    public BigInteger AnnualInsurance { get; set; }

    [Parameter("uint256", "holdersPct", 3)]
    public BigInteger HoldersPct { get; set; }

    [Parameter("uint256", "buyerInterest", 4)]
    public BigInteger BuyerInterest { get; set; }

    [Parameter("uint256", "brickleInterest", 5)]
    public BigInteger BrickleInterest { get; set; }

    [Parameter("uint256", "principal", 6)]
    public BigInteger Principal { get; set; }

    [Parameter("uint256", "totalMonthlyPayment", 7)]
    public BigInteger TotalMonthlyPayment { get; set; }

    [Parameter("uint256", "monthlyRateBuyer", 8)]
    public BigInteger MonthlyRateBuyer { get; set; }
}
