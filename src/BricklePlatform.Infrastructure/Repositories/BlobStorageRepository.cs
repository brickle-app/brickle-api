using Azure.Storage.Blobs;
using BricklePlatform.Infrastructure.Constants;
using BricklePlatform.Infrastructure.Interfaces;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BricklePlatform.Infrastructure.Repositories;

public class BlobStorageRepository : BaseBlobRepository, IBlobStorageRepository
{
    private readonly ILogger<BlobStorageRepository> _logger;

    public BlobStorageRepository(
        IOptions<InfrastructureSettings> settings,
        ILogger<BlobStorageRepository> logger
    ) : base(settings, logger)
    {
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(string entityType, Guid entityId, string fileType, byte[] fileBytes, string fileName)
    {
        try
        {
            // Asegurar que la carpeta existe
            await CreateEntityFolderAsync(entityType, entityId);

            // Obtener la extensión del archivo
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".jpg"; // Extensión por defecto
            }

            // Construir la ruta del archivo
            string blobPath = BlobConstants.BuildFilePath(entityType, entityId, fileType, extension);

            // Subir el archivo
            return await UploadBytesAsync(fileBytes, blobPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al subir archivo tipo {FileType} para entidad {EntityType} con ID {EntityId}",
                fileType, entityType, entityId);
            throw;
        }
    }

    public async Task<string?> GetLatestFileUrlAsync(string entityType, Guid entityId, string fileType)
    {
        try
        {
            BlobContainerClient containerClient = GetContainerClient();
            string folderPath = BlobConstants.GetEntityFolderPath(entityType, entityId);

            Azure.AsyncPageable<Azure.Storage.Blobs.Models.BlobItem> resultSegment = containerClient.GetBlobsAsync(prefix: folderPath);
            List<(string Name, DateTimeOffset LastModified)> files = new List<(string Name, DateTimeOffset LastModified)>();

            await foreach (Azure.Storage.Blobs.Models.BlobItem blobItem in resultSegment)
            {
                if (!blobItem.Name.EndsWith(BlobConstants.FOLDER_MARKER) &&
                    blobItem.Name.Contains(fileType))
                {
                    files.Add((blobItem.Name, blobItem.Properties.LastModified ?? DateTimeOffset.MinValue));
                }
            }

            if (files.Any())
            {
                (string Name, DateTimeOffset LastModified) latestFile = files.OrderByDescending(p => p.LastModified).First();
                return await GetBlobUrl(latestFile.Name);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al obtener URL del archivo tipo {FileType} para entidad {EntityType} con ID {EntityId}",
                fileType, entityType, entityId);
            return null;
        }
    }

    public async Task<bool> DeleteEntityFilesAsync(string entityType, Guid entityId)
    {
        try
        {
            string folderPath = BlobConstants.GetEntityFolderPath(entityType, entityId);
            return await DeleteFolderAsync(folderPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al eliminar archivos de la entidad {EntityType} con ID {EntityId}",
                entityType, entityId);
            return false;
        }
    }

    private async Task<bool> CreateEntityFolderAsync(string entityType, Guid entityId)
    {
        string folderPath = BlobConstants.GetEntityFolderPath(entityType, entityId);
        return await CreateFolderAsync(folderPath);
    }

    public override async Task<string> UploadBytesAsync(byte[] fileBytes, string fileName)
    {
        try
        {
            BlobClient blobClient = _containerClient.GetBlobClient(fileName);
            using MemoryStream stream = new MemoryStream(fileBytes);
            await blobClient.UploadAsync(stream, overwrite: true);

            return await GetBlobUrl(fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir el archivo {FileName}", fileName);
            throw;
        }
    }
}