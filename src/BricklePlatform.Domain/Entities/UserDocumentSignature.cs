namespace BricklePlatform.Domain.Entities;

/// <summary>
/// Evidencia de firma electrónica (Ley 527 de 1999) capturada en pantalla por el
/// usuario al aceptar un documento legal (p. ej. contrato de colaboración
/// empresarial, declaración de origen de fondos) que no se sirve como PDF externo.
/// </summary>
public class UserDocumentSignature
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string DocumentType { get; private set; } = null!;
    /// <summary>Versión/hash del texto legal que se firmó, para trazabilidad si el texto cambia.</summary>
    public string DocumentVersion { get; private set; } = null!;
    /// <summary>Trazos de la firma capturados en el lienzo, serializados como JSON (arreglo de paths SVG).</summary>
    public string SignatureData { get; private set; } = null!;
    public string SignerName { get; private set; } = null!;
    public string? IpAddress { get; private set; }
    public DateTime SignedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation property
    public User? User { get; private set; }

    private UserDocumentSignature() { }

    public static UserDocumentSignature Create(
        Guid userId,
        string documentType,
        string documentVersion,
        string signatureData,
        string signerName,
        string? ipAddress)
    {
        var now = DateTime.UtcNow;
        return new UserDocumentSignature
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DocumentType = documentType,
            DocumentVersion = documentVersion,
            SignatureData = signatureData,
            SignerName = signerName,
            IpAddress = ipAddress,
            SignedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void ReSign(
        string documentVersion,
        string signatureData,
        string signerName,
        string? ipAddress)
    {
        DocumentVersion = documentVersion;
        SignatureData = signatureData;
        SignerName = signerName;
        IpAddress = ipAddress;
        SignedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
