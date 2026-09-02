using BricklePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BricklePlatform.Infrastructure.Persistence.Configurations;

public class UserDocumentSignatureConfiguration : IEntityTypeConfiguration<UserDocumentSignature>
{
    public void Configure(EntityTypeBuilder<UserDocumentSignature> builder)
    {
        builder.ToTable("UserDocumentSignature", "dbo");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.Property(s => s.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(s => s.DocumentType)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("document_type");

        builder.Property(s => s.DocumentVersion)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("document_version");

        builder.Property(s => s.SignatureData)
            .IsRequired()
            .HasColumnType("nvarchar(max)")
            .HasColumnName("signature_data");

        builder.Property(s => s.SignerName)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("signer_name");

        builder.Property(s => s.IpAddress)
            .IsRequired(false)
            .HasMaxLength(64)
            .HasColumnName("ip_address");

        builder.Property(s => s.SignedAt)
            .IsRequired()
            .HasColumnName("signed_at");

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(s => s.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.UserId, s.DocumentType })
            .IsUnique()
            .HasDatabaseName("IX_UserDocumentSignature_UserId_DocumentType");
    }
}
