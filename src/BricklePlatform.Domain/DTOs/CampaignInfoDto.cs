using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;

namespace BricklePlatform.Domain.DTOs;

[Struct("CampaignInfo")]
public class CampaignInfoDto
{
    [Parameter("uint256", "minCap", 1)]
    public BigInteger minCap { get; set; }

    [Parameter("uint256", "maxCap", 2)]
    public BigInteger maxCap { get; set; }

    [Parameter("uint256", "totalLeasingTokens", 3)]
    public BigInteger totalLeasingTokens { get; set; }

    [Parameter("uint256", "tokenPrice", 4)]
    public BigInteger tokenPrice { get; set; }

    [Parameter("uint256", "deadline", 5)]
    public BigInteger deadline { get; set; }

    [Parameter("address", "baseToken", 6)]
    public string baseToken { get; set; }

    [Parameter("address", "brickleAddress", 7)]
    public string brickleAddress { get; set; }
}