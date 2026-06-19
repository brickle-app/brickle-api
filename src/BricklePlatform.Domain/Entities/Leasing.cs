using BricklePlatform.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace BricklePlatform.Domain.Entities;

public class AssetDetail : IEquatable<AssetDetail>
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public bool Equals(AssetDetail? other)
    {
        if (other == null) return false;
        return Title == other.Title && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as AssetDetail);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Title, Value);
    }
}

public class Leasing
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public int Tokens { get; private set; }
    public int TokensAvailable { get; private set; }
    public decimal PricePerToken { get; private set; }
    public string? Description { get; private set; }
    public LeasingTypeEnum Type { get; private set; }
    public DateTime? ContractTime { get; private set; }
    public LiquidityLevelEnum Liquidity { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public string? MiniatureImageUrl { get; private set; }
    public string? DiscoverImageUrl { get; private set; }
    public string ContractAddress { get; private set; }
    public decimal TIR { get; private set; }
    public bool Active { get; private set; }
    public decimal ReteIcaPct { get; private set; }
    public decimal ReteFuentePct { get; private set; }
    public List<AssetDetail>? Details { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private Leasing()
    { }

    public static Leasing Create(
        string name,
        int quantity,
        decimal price,
        int tokens,
        int tokensAvailable,
        decimal pricePerToken,
        decimal tir,
        LeasingTypeEnum type,
        LiquidityLevelEnum liquidity,
        bool active = true,
        string? description = null,
        DateTime? contractTime = null,
        string? coverImageUrl = null,
        string? miniatureImageUrl = null,
        string? discoverImageUrl = null,
        string? leasingContractAddress = null,
        List<AssetDetail>? details = null,
        decimal reteIcaPct = 0,
        decimal reteFuentePct = 0
        )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (price <= 0)
            throw new ArgumentException("Price must be greater than 0", nameof(price));

        if (tokens <= 0)
            throw new ArgumentException("Tokens must be greater than 0", nameof(tokens));

        if (tokensAvailable <= 0)
            throw new ArgumentException("Tokens available must be greater than 0", nameof(tokensAvailable));

        if (pricePerToken <= 0)
            throw new ArgumentException("Price per token must be greater than 0", nameof(pricePerToken));

        if (tir < 0)
            throw new ArgumentException("TIR must be greater than or equal to 0", nameof(tir));

        return new Leasing
        {
            Id = Guid.NewGuid(),
            Name = name,
            Quantity = quantity,
            Price = price,
            Tokens = tokens,
            TokensAvailable = tokensAvailable,
            PricePerToken = pricePerToken,
            Description = description,
            Type = type,
            ContractTime = contractTime,
            Liquidity = liquidity,
            CoverImageUrl = coverImageUrl,
            MiniatureImageUrl = miniatureImageUrl,
            DiscoverImageUrl = discoverImageUrl,
            ContractAddress = leasingContractAddress,
            TIR = tir,
            Active = active,
            Details = details,
            ReteIcaPct = reteIcaPct,
            ReteFuentePct = reteFuentePct,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        int quantity,
        decimal price,
        int tokens,
        int tokensAvailable,
        decimal pricePerToken,
        LeasingTypeEnum type,
        LiquidityLevelEnum liquidity,
        decimal tir,
        string? description = null,
        DateTime? contractTime = null,
        string? coverImageUrl = null,
        string? miniatureImageUrl = null,
        string? discoverImageUrl = null,
        string? leasingContractAddress = null,
        List<AssetDetail>? details = null,
        decimal reteIcaPct = 0,
        decimal reteFuentePct = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (price <= 0)
            throw new ArgumentException("Price must be greater than 0", nameof(price));

        if (tokens <= 0)
            throw new ArgumentException("Tokens must be greater than 0", nameof(tokens));

        if (tokensAvailable < 0)
            throw new ArgumentException("Tokens available cannot be negative", nameof(tokensAvailable));

        if (pricePerToken <= 0)
            throw new ArgumentException("Price per token must be greater than 0", nameof(pricePerToken));

        if (tir < 0)
            throw new ArgumentException("TIR must be greater than or equal to 0", nameof(tir));

        Name = name;
        Quantity = quantity;
        Price = price;
        Tokens = tokens;
        TokensAvailable = tokensAvailable;
        PricePerToken = pricePerToken;
        Description = description;
        Type = type;
        ContractTime = contractTime;
        Liquidity = liquidity;
        CoverImageUrl = coverImageUrl;
        MiniatureImageUrl = miniatureImageUrl;
        DiscoverImageUrl = discoverImageUrl;
        ContractAddress = leasingContractAddress;
        TIR = tir;
        Details = details;
        ReteIcaPct = reteIcaPct;
        ReteFuentePct = reteFuentePct;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContractAddress(string contractAddress)
    {
        if (string.IsNullOrWhiteSpace(contractAddress))
            throw new ArgumentException("Contract address cannot be empty", nameof(contractAddress));

        ContractAddress = contractAddress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCoverImage(string coverImageUrl)
    {
        CoverImageUrl = coverImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMiniatureImage(string miniatureImageUrl)
    {
        MiniatureImageUrl = miniatureImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDiscoverImage(string discoverImageUrl)
    {
        DiscoverImageUrl = discoverImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTokensAvailable(int tokensAvailable)
    {
        if (tokensAvailable < 0)
            throw new ArgumentException("Tokens available cannot be negative", nameof(tokensAvailable));

        if (tokensAvailable > Tokens)
            throw new ArgumentException("Tokens available cannot exceed total tokens", nameof(tokensAvailable));

        TokensAvailable = tokensAvailable;
        UpdatedAt = DateTime.UtcNow;
    }
}