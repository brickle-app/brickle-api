using BricklePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BricklePlatform.Infrastructure.Persistence.Configurations;

public class WalletBackupConfiguration : IEntityTypeConfiguration<WalletBackup>
{
    public void Configure(EntityTypeBuilder<WalletBackup> builder)
    {
        builder.ToTable("WalletBackup", "dbo");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id).HasColumnName("id");
        builder.Property(w => w.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(w => w.WalletAddress).HasColumnName("wallet_address").HasMaxLength(42).IsRequired();
        builder.Property(w => w.EncryptedPrivateKey).HasColumnName("encrypted_private_key").HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(w => w.EncryptionVersion).HasColumnName("encryption_version").HasMaxLength(64).IsRequired();
        builder.Property(w => w.Cipher).HasColumnName("cipher").HasMaxLength(64).IsRequired();
        builder.Property(w => w.Kdf).HasColumnName("kdf").HasMaxLength(64).IsRequired();
        builder.Property(w => w.KdfParamsJson).HasColumnName("kdf_params_json").HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(w => w.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(w => w.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(w => w.LastRestoredAt).HasColumnName("last_restored_at");

        builder.HasIndex(w => w.UserId).IsUnique();
        builder.HasIndex(w => w.WalletAddress).IsUnique();

        builder.HasOne(w => w.User)
            .WithOne()
            .HasForeignKey<WalletBackup>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
