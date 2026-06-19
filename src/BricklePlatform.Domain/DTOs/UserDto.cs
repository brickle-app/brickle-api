using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Domain.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? WalletAddress { get; set; }

    // Propiedades requeridas para completar perfil básico
    public string PhoneNumber { get; set; } = string.Empty;
    public bool TermsAccepted { get; set; }

    // Propiedades opcionales para completar perfil
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

    public DateTime CreatedAt { get; set; }

    // Propiedades calculadas para verificar completitud del perfil
    public bool IsBasicProfileComplete { get; set; }
    public bool IsFullProfileComplete { get; set; }
    public bool IsProfileUnderReview { get; set; }

    // Empresa asociada (opcional - relación 1:1)
    public CompanyDto? Company { get; set; }

    // Propiedad calculada para obtener el nombre completo
    public string FullName => $"{FirstName} {LastName}".Trim();
}