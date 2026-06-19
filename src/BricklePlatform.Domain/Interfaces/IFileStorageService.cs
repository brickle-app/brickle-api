using BricklePlatform.Domain.Models;

namespace BricklePlatform.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(FileData file, Guid entityId, string propertyName);
}