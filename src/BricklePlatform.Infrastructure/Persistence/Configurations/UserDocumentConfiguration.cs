using BricklePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BricklePlatform.Infrastructure.Persistence.Configurations;

public class UserDocumentConfiguration : IEntityTypeConfiguration<UserDocument>
{
    public void Configure(EntityTypeBuilder<UserDocument> builder)
    {
        builder.ToTable("UserDocument", "dbo");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.Property(d => d.UserId)
            .IsRequired(false)
            .HasColumnName("user_id");

        builder.Property(d => d.Name)
            .IsRequired(false)
            .HasMaxLength(200)
            .HasColumnName("document_name");

        builder.Property(d => d.DocumentUrl)
            .IsRequired(false)
            .HasMaxLength(500)
            .HasColumnName("document_url");

        builder.Property(d => d.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("status");

        builder.Property(d => d.Observation)
            .HasMaxLength(500)
            .HasColumnName("observation");

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.UserId)
            .HasDatabaseName("IX_UserDocument_UserId");
    }
}
