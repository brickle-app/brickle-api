using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public byte[] PasswordHash { get; private set; }
    public byte[] PasswordSalt { get; private set; }
    public string? WalletAddress { get; private set; }
    public string PhoneNumber { get; private set; }
    public bool TermsAccepted { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Nationality { get; private set; }
    public string? CountryOfResidence { get; private set; }
    public DocumentTypeEnum? DocumentType { get; private set; }
    public string? DocumentNumber { get; private set; }
    public string? KycCustomerId { get; private set; }
    public string? KycSubmissionId { get; private set; }
    public string? PushNotificationToken { get; private set; }
    public string? CurrentSession { get; private set; }
    public string? ExternalWalletId { get; private set; }
    public bool IsBasicProfileComplete { get; set; }
    public bool IsFullProfileComplete { get; set; }
    public bool IsProfileUnderReview { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation property - optional Company (1:1 relationship)
    public Company? Company { get; private set; }

    private User()
    { }

    public static User Create(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        bool termsAccepted,
        byte[] passwordHash,
        byte[] passwordSalt,
        string? profilePictureUrl = null,
        string? walletAddress = null,
        DateTime? dateOfBirth = null,
        string? nationality = null,
        string? countryOfResidence = null,
        DocumentTypeEnum? documentType = null,
        string? documentNumber = null,
        string? kycCustomerId = null,
        string? kycSubmissionId = null,
        string? pushNotificationToken = null,
        string? currentSession = null,
        string? externalWalletId = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            TermsAccepted = termsAccepted,
            ProfilePictureUrl = profilePictureUrl,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            WalletAddress = walletAddress,
            DateOfBirth = dateOfBirth,
            Nationality = nationality,
            CountryOfResidence = countryOfResidence,
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            KycCustomerId = kycCustomerId,
            KycSubmissionId = kycSubmissionId,
            PushNotificationToken = pushNotificationToken,
            CurrentSession = currentSession,
            ExternalWalletId = externalWalletId,
            CreatedAt = DateTime.UtcNow,
            IsBasicProfileComplete = false,
            IsFullProfileComplete = false,
            IsProfileUnderReview = false
        };
    }

    public void Update(
        string? firstName = null,
        string? lastName = null,
        string? phoneNumber = null,
        string? profilePictureUrl = null,
        string? walletAddress = null,
        DateTime? dateOfBirth = null,
        string? nationality = null,
        string? countryOfResidence = null,
        DocumentTypeEnum? documentType = null,
        string? documentNumber = null,
        string? kycCustomerId = null,
        string? kycSubmissionId = null,
        string? pushNotificationToken = null,
        string? currentSession = null,
        string? externalWalletId = null,
        bool? isBasicProfileComplete = null,
        bool? isFullProfileComplete = null,
        bool? isProfileUnderReview = null)
    {
        if (firstName != null) FirstName = firstName;
        if (lastName != null) LastName = lastName;
        if (phoneNumber != null) PhoneNumber = phoneNumber;
        if (profilePictureUrl != null) ProfilePictureUrl = profilePictureUrl;
        if (walletAddress != null) WalletAddress = walletAddress;
        if (dateOfBirth != null) DateOfBirth = dateOfBirth;
        if (nationality != null) Nationality = nationality;
        if (countryOfResidence != null) CountryOfResidence = countryOfResidence;
        if (documentType != null) DocumentType = documentType;
        if (documentNumber != null) DocumentNumber = documentNumber;
        if (kycCustomerId != null) KycCustomerId = kycCustomerId;
        if (kycSubmissionId != null) KycSubmissionId = kycSubmissionId;
        if (pushNotificationToken != null) PushNotificationToken = pushNotificationToken;
        if (currentSession != null) CurrentSession = currentSession;
        if (externalWalletId != null) ExternalWalletId = externalWalletId;
        if (isBasicProfileComplete.HasValue) IsBasicProfileComplete = isBasicProfileComplete.Value;
        if (isFullProfileComplete.HasValue) IsFullProfileComplete = isFullProfileComplete.Value;
        if (isProfileUnderReview.HasValue) IsProfileUnderReview = isProfileUnderReview.Value;

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfilePicture(string profilePictureUrl)
    {
        ProfilePictureUrl = profilePictureUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(byte[] newPasswordHash, byte[] newPasswordSalt)
    {
        PasswordHash = newPasswordHash;
        PasswordSalt = newPasswordSalt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCurrentSession(string? currentSession)
    {
        CurrentSession = currentSession;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateExternalWalletId(string? externalWalletId)
    {
        ExternalWalletId = externalWalletId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearSensitiveData()
    {
        PasswordHash = Array.Empty<byte>();
        PasswordSalt = Array.Empty<byte>();
    }
}