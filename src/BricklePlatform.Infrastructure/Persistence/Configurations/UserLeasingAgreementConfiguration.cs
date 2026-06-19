using BricklePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BricklePlatform.Infrastructure.Persistence.Configurations;

public class UserLeasingAgreementConfiguration : IEntityTypeConfiguration<UserLeasingAgreement>
{
    public void Configure(EntityTypeBuilder<UserLeasingAgreement> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(a => a.LeasingId)
            .IsRequired()
            .HasColumnName("leasing_id");

        builder.Property(a => a.AssetValue)
            .IsRequired()
            .HasPrecision(18, 6)
            .HasColumnName("asset_value");

        builder.Property(a => a.UsefulLife)
            .IsRequired()
            .HasColumnName("useful_life");

        builder.Property(a => a.TermTime)
            .IsRequired()
            .HasPrecision(18, 6)
            .HasColumnName("term_time");

        builder.Property(a => a.PaymentTerm)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("payment_term");

        builder.Property(a => a.AgreementType)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("agreement_type");

        builder.Property(a => a.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnName("currency");

        builder.Property(a => a.ContractDetails)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("contract_details");

        builder.Property(a => a.StartDate)
            .IsRequired()
            .HasColumnName("start_date");

        builder.Property(a => a.EndDate)
            .IsRequired()
            .HasColumnName("end_date");

        builder.Property(a => a.InstallmentRate)
            .IsRequired()
            .HasPrecision(18, 6)
            .HasColumnName("installment_rate");

        builder.Property(a => a.InstallmentAmount)
            .IsRequired()
            .HasPrecision(18, 6)
            .HasColumnName("installment_amount");

        builder.Property(a => a.ManagementFee)
            .IsRequired()
            .HasPrecision(18, 6)
            .HasColumnName("management_fee");

        builder.Property(a => a.TotalValue)
            .IsRequired()
            .HasPrecision(18, 6)
            .HasColumnName("total_value");

        builder.Property(a => a.RemainingBalance)
            .IsRequired()
            .HasPrecision(18, 6)
            .HasColumnName("remaining_balance");

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("status");

        builder.Property(a => a.LeasingCoreAddress)
           .IsRequired()
           .HasMaxLength(50)
           .HasColumnName("leasing_address");

        builder.Property(a => a.InsurancePercentage)
            .IsRequired()
            .HasPrecision(18, 6)
            .HasColumnName("insurance_percentage");

        builder.Property(a => a.IbrRate)
            .IsRequired()
            .HasPrecision(18, 6)
            .HasColumnName("ibr_rate");

        builder.Property(a => a.RiskLevel)
            .IsRequired()
            .HasPrecision(18, 6)
            .HasColumnName("risk_level");

        builder.Property(a => a.RiskRate)
            .IsRequired()
            .HasPrecision(18, 6)
            .HasColumnName("risk_rate");

        builder.Property(a => a.IVA)
            .IsRequired()
            .HasPrecision(18, 6)
            .HasColumnName("iva");

        builder.Property(a => a.BuyerRetentionPercentage)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m)
            .HasColumnName("buyer_retention_percentage");

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(a => a.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Leasing)
            .WithMany()
            .HasForeignKey(a => a.LeasingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}