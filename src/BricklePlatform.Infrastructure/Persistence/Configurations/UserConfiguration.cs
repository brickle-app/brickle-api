using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BricklePlatform.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("first_name");

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("last_name");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("email");

        builder.Property(u => u.ProfilePictureUrl)
            .HasMaxLength(255)
            .HasColumnName("profile_picture_url");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasColumnName("password_hash");

        builder.Property(u => u.PasswordSalt)
            .IsRequired()
            .HasColumnName("password_salt");

        builder.Property(u => u.WalletAddress)
            .HasMaxLength(42)
            .HasColumnName("wallet_address");

        // Propiedades requeridas para completar perfil básico
        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("phone_number");

        builder.Property(u => u.TermsAccepted)
            .IsRequired()
            .HasColumnName("terms_accepted");

        // Propiedades opcionales para completar perfil
        builder.Property(u => u.DateOfBirth)
            .HasColumnName("date_of_birth");

        builder.Property(u => u.Nationality)
            .HasMaxLength(100)
            .HasColumnName("nationality");

        builder.Property(u => u.CountryOfResidence)
            .HasMaxLength(100)
            .HasColumnName("country_of_residence");

        builder.Property(u => u.DocumentType)
            .HasColumnName("document_type")
            .HasConversion<int>();

        builder.Property(u => u.DocumentNumber)
            .HasMaxLength(50)
            .HasColumnName("document_number");

        builder.Property(u => u.KycCustomerId)
            .HasMaxLength(255)
            .HasColumnName("kyc_customer_id");

        builder.Property(u => u.KycSubmissionId)
            .HasMaxLength(100)
            .HasColumnName("kyc_submission_id");

        builder.Property(u => u.PushNotificationToken)
            .HasMaxLength(100)
            .HasColumnName("push_notification_token");

        builder.Property(u => u.CurrentSession)
            .HasColumnType("nvarchar(max)")
            .HasColumnName("current_session");

        builder.Property(u => u.ExternalWalletId)
            .HasMaxLength(255)
            .HasColumnName("external_wallet_id");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(u => u.IsBasicProfileComplete)
            .IsRequired()
            .HasColumnName("is_basic_profile_complete");

        builder.Property(u => u.IsFullProfileComplete)
            .IsRequired()
            .HasColumnName("is_full_profile_complete");

        builder.Property(u => u.IsProfileUnderReview)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("is_profile_under_review");

        // Configure unique constraints
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Configure unique constraint for document number when not null
        builder.HasIndex(u => u.DocumentNumber)
            .IsUnique()
            .HasFilter("[document_number] IS NOT NULL");
    }
}