using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Domain.DTOs;

public class CreateLeasingDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public int Tokens { get; set; }
    public int TokensAvailable { get; set; }
    public decimal PricePerToken { get; set; }
    public string? Description { get; set; }
    public LeasingTypeEnum Type { get; set; }
    public DateTime? ContractTime { get; set; }
    public LiquidityLevelEnum Liquidity { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? MiniatureImageUrl { get; set; }
    public string? DiscoverImageUrl { get; set; }
    public string? ContractAddress { get; set; } = string.Empty;
    public decimal TIR { get; set; }
    public bool Active { get; set; } = true;
    public decimal ReteIcaPct { get; set; }
    public decimal ReteFuentePct { get; set; }
    public List<AssetDetailDto>? Details { get; set; }
}