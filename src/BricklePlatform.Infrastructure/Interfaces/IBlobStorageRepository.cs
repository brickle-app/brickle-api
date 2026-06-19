namespace BricklePlatform.Infrastructure.Interfaces;

public interface IBlobStorageRepository
{
    Task<string> UploadFileAsync(string entityType, Guid entityId, string fileType, byte[] fileBytes, string fileName);
    Task<string?> GetLatestFileUrlAsync(string entityType, Guid entityId, string fileType);
    Task<bool> DeleteEntityFilesAsync(string entityType, Guid entityId);
}