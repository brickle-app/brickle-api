using BricklePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace BricklePlatform.Infrastructure.Persistence.Configurations;

public class LeasingConfiguration : IEntityTypeConfiguration<Leasing>
{
    public void Configure(EntityTypeBuilder<Leasing> builder)
    {
        builder.ToTable("Leasing");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("NEWID()");

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(l => l.Price)
            .HasColumnName("price")
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(l => l.Tokens)
            .HasColumnName("tokens")
            .IsRequired();

        builder.Property(l => l.ContractAddress)
           .HasColumnName("contract_address")
           .IsRequired();

        builder.Property(l => l.TokensAvailable)
            .HasColumnName("tokens_available")
            .IsRequired();

        builder.Property(l => l.PricePerToken)
            .HasColumnName("price_per_token")
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(l => l.TIR)
            .HasColumnName("TIR")
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(l => l.Description)
            .HasColumnName("description")
            .HasMaxLength(200);

        builder.Property(l => l.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.ContractTime)
            .HasColumnName("contract_time");

        builder.Property(l => l.Liquidity)
            .HasColumnName("liquidity")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.CoverImageUrl)
            .HasColumnName("cover_image_url")
            .HasMaxLength(255);

        builder.Property(l => l.MiniatureImageUrl)
            .HasColumnName("miniature_image_url")
            .HasMaxLength(255);

        builder.Property(l => l.TIR)
            .HasColumnName("TIR")
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(l => l.Active)
            .HasColumnName("active")
            .HasDefaultValue(true);

        builder.Property(l => l.ReteIcaPct)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        builder.Property(l => l.ReteFuentePct)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        builder.Property(l => l.Details)
            .HasColumnName("details")
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<AssetDetail>>(v, (JsonSerializerOptions?)null),
                new ValueComparer<List<AssetDetail>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("GETDATE()");
    }
}