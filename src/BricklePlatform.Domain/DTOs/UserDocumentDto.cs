namespace BricklePlatform.Domain.DTOs;

public class UserDocumentDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public string? Name { get; set; }
    public string? DocumentUrl { get; set; }
    public string Status { get; set; } = null!;
    public string? Observation { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
