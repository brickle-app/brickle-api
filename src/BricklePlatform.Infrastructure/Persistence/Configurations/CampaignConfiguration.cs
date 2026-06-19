using BricklePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BricklePlatform.Infrastructure.Persistence.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaign");

        builder.HasKey(c => c.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(c => c.LeasingId)
            .IsRequired()
            .HasColumnName("leasing_id");

        builder.Property(c => c.MinCapital)
            .IsRequired()
            .HasColumnName("min_capital");

        builder.Property(c => c.MaxCapital)
            .IsRequired()
            .HasColumnName("max_capital");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasColumnName("status");

        builder.Property(c => c.BaseToken)
            .IsRequired()
            .HasColumnName("base_token");

        builder.Property(c => c.BrickleAddress)
            .IsRequired()
            .HasColumnName("brickle_address");

        builder.Property(c => c.CampaignAddress)
            .IsRequired()
            .HasColumnName("campaign_address");

        builder.Property(c => c.CampaignTx)
            .IsRequired()
            .HasColumnName("campaign_tx");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at")
            .HasDefaultValueSql("GETDATE()");

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasColumnName("update_at")
            .HasDefaultValueSql("GETDATE()");

        builder.HasOne(c => c.Leasing)
            .WithMany()
            .HasForeignKey(c => c.LeasingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}