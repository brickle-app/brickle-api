using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Domain.DTOs;

public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    
    // Propiedades requeridas para completar perfil básico
    public string PhoneNumber { get; set; } = string.Empty;
    public bool TermsAccepted { get; set; }
    
    // Propiedades opcionales
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
}

