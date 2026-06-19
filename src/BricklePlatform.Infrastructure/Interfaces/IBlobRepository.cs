using Azure.Storage.Blobs;

namespace BricklePlatform.Infrastructure.Interfaces;

public interface IBlobRepository
{
    Task<string> UploadBytesAsync(byte[] fileBytes, string fileName);
    Task<string> GetBlobUrl(string fileName);
    Task<bool> CreateFolderAsync(string folderPath);
    Task<bool> DeleteFolderAsync(string folderPath);
    Task<bool> FolderExistsAsync(string folderPath);
    BlobContainerClient GetContainerClient();
}