namespace BricklePlatform.Domain.DTOs;

public class UserDocumentSignatureDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DocumentType { get; set; } = null!;
    public string DocumentVersion { get; set; } = null!;
    public string SignerName { get; set; } = null!;
    public DateTime SignedAt { get; set; }
}
