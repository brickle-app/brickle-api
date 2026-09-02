namespace BricklePlatform.Api.Application.Dtos;

public class SignUserDocumentRequestDto
{
    public Guid UserId { get; set; }
    public string DocumentType { get; set; } = null!;
    /// <summary>Versión del texto legal mostrado al usuario en el momento de la firma.</summary>
    public string DocumentVersion { get; set; } = null!;
    /// <summary>Trazos de la firma capturados en pantalla (arreglo de paths SVG "d").</summary>
    public List<string> SignaturePaths { get; set; } = new();
    public string SignerName { get; set; } = null!;
}
