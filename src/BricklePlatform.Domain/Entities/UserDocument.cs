using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Domain.Entities;

public class UserDocument
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string? Name { get; private set; }
    public string DocumentType { get; private set; } = UserDocumentType.Identity;
    public string? DocumentUrl { get; private set; }
    public string Status { get; private set; }
    public string? Observation { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation property
    public User? User { get; private set; }

    private UserDocument() { }

    public static UserDocument Create(
        Guid userId,
        string name,
        string documentType,
        string documentUrl)
    {
        return new UserDocument
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            DocumentType = documentType,
            DocumentUrl = documentUrl,
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(string status, string? observation = null)
    {
        Status = status;
        Observation = observation;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resubmit(string name, string documentUrl)
    {
        Name = name;
        DocumentUrl = documentUrl;
        Status = "PENDING";
        Observation = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
