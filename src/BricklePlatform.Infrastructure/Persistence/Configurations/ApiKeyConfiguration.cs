using BricklePlatform.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BricklePlatform.Infrastructure.Persistence.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("Keys");

        builder.HasKey(k => k.Id);
        builder.Property(k => k.Id)
            .HasColumnName("Id")
            .UseIdentityColumn();

        builder.Property(k => k.Application)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("Aplication");

        builder.Property(k => k.Key)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("Key");

        builder.Property(k => k.IsActive)
            .IsRequired()
            .HasColumnName("IsActive");

        builder.Property(k => k.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt")
            .HasDefaultValueSql("GETDATE()");

        builder.Property(k => k.ExpiresAt)
            .HasColumnName("ExpiresAt");

        // Configure unique constraints
        builder.HasIndex(k => k.Key)
            .IsUnique();
    }
}