using BricklePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BricklePlatform.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Company");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("name");

        builder.Property(c => c.OperationTime)
            .IsRequired()
            .HasColumnName("operation_time");

        builder.Property(c => c.OperationMeasure)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("operation_measure");

        builder.Property(c => c.CreditRating)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("credit_rating");

        builder.Property(c => c.LeasingContract)
            .HasMaxLength(500)
            .HasColumnName("leasing_contract");

        builder.Property(c => c.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // Configure 1:1 relationship with User
        builder.HasOne(c => c.User)
            .WithOne(u => u.Company)
            .HasForeignKey<Company>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure unique constraint for UserId (ensures 1:1 relationship)
        builder.HasIndex(c => c.UserId)
            .IsUnique();
    }
}