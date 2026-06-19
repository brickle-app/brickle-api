namespace BricklePlatform.Domain.DTOs;

public class AssetDetailDto
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class LeasingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public int Tokens { get; set; }
    public int TokensAvailable { get; set; }
    public decimal PricePerToken { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime? ContractTime { get; set; }
    public string Liquidity { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string? MiniatureImageUrl { get; set; }
    public string? DiscoverImageUrl { get; set; }
    public string ContractAddress { get; set; } = string.Empty;
    public decimal TIR { get; set; }
    public decimal ReteIcaPct { get; set; }
    public decimal ReteFuentePct { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public List<AssetDetailDto>? Details { get; set; }
    public UserLeasingAgreementInfoDto? Agreement { get; set; }
    public CompanyDto? Company { get; set; }
}