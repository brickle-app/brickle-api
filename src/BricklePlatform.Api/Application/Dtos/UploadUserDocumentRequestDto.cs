using Microsoft.AspNetCore.Http;

namespace BricklePlatform.Api.Application.Dtos;

public class UploadUserDocumentRequestDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public IFormFile File { get; set; } = null!;
}
