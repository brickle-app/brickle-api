using BricklePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BricklePlatform.Infrastructure.Persistence.Configurations;

public class UserBankAccountConfiguration : IEntityTypeConfiguration<UserBankAccount>
{
    public void Configure(EntityTypeBuilder<UserBankAccount> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(x => x.BankName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("bank_name");

        builder.Property(x => x.AccountType)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("account_type");

        builder.Property(x => x.AccountNumber)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("account_number");

        builder.Property(x => x.AccountHolder)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("account_holder");

        builder.Property(x => x.AccountDocument)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("account_document");

        builder.Property(x => x.AccountImage)
            .HasMaxLength(500)
            .IsRequired(false)
            .HasColumnName("account_image");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_UserBankAccount_UserId");
    }
}