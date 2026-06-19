namespace BricklePlatform.Domain.Interfaces;

public interface IFileService
{
    Task<(bool IsValid, string? ErrorMessage)> ValidateFileAsync(Stream? fileStream, string fileName);
    Task<string> UploadFileAsync(string entityType, Guid entityId, string fileType, Stream fileStream, string fileName);
    Task<string?> GetFileUrlAsync(string entityType, Guid entityId, string fileType);
}