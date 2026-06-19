using Microsoft.AspNetCore.Http;

namespace BricklePlatform.Api.Application.Dtos;

public class UploadFileRequestDto
{
    public required Guid EntityId { get; set; }
    public required IFormFile File { get; set; }
}