using BricklePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BricklePlatform.Infrastructure.Persistence.Configurations
{
    public class InvestmentConfiguration : IEntityTypeConfiguration<Investment>
    {
        public void Configure(EntityTypeBuilder<Investment> builder)
        {
            builder.ToTable("Investment", "dbo");

            builder.HasKey(i => i.Id)
                .HasName("PK_Investment");

            builder.Property(i => i.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(i => i.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(i => i.LeasingId)
                .HasColumnName("leasing_id")
                .IsRequired();

            builder.Property(i => i.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,6)")
                .HasPrecision(18, 6)
                .IsRequired();

            builder.Property(i => i.BricksCount)
                .HasColumnName("bricks_count")
                .IsRequired();

            builder.Property(i => i.BricksName)
                .HasColumnName("bricks_name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(i => i.PaymentCount)
                .HasColumnName("payment_count")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(i => i.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(i => i.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            builder.HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Leasing)
                .WithMany()
                .HasForeignKey(i => i.LeasingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(i => i.UserId)
                .HasDatabaseName("IX_Investment_user_id");

            builder.HasIndex(i => i.LeasingId)
                .HasDatabaseName("IX_Investment_leasing_id");
        }
    }
}