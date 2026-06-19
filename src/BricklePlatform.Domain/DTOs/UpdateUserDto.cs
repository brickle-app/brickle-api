using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Domain.DTOs;

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? WalletAddress { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? CountryOfResidence { get; set; }
    public DocumentTypeEnum? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? KycCustomerId { get; set; }
    public string? KycSubmissionId { get; set; }
    public string? PushNotificationToken { get; set; }
    public string? CurrentSession { get; set; }
    public string? ExternalWalletId { get; set; }
    public bool? IsBasicProfileComplete { get; set; }
    public bool? IsFullProfileComplete { get; set; }
    public bool? IsProfileUnderReview { get; set; }
    public string? Email { get; set; }
    public bool? TermsAccepted { get; set; }
}