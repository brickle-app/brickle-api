using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using BricklePlatform.Infrastructure.Interfaces;
using BricklePlatform.Infrastructure.Services.Base.Blobs;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BricklePlatform.Infrastructure.Repositories;

public abstract class BaseBlobRepository : ContainerStorage, IBlobRepository
{
    private readonly ILogger<BaseBlobRepository> _logger;
    protected readonly BlobContainerClient _containerClient;

    protected BaseBlobRepository(
        IOptions<InfrastructureSettings> settings,
        ILogger<BaseBlobRepository> logger
    ) : base(settings.Value.AzureSettings.ConnectionString,
             settings.Value.AzureSettings.BlobName)
    {
        _logger = logger;
        _containerClient = new BlobContainerClient(
            settings.Value.AzureSettings.ConnectionString,
            settings.Value.AzureSettings.BlobName);
    }

    public virtual async Task<string> UploadBytesAsync(byte[] fileBytes, string fileName)
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

    public async Task<string> GetBlobUrl(string fileName)
    {
        return await GetBlob(fileName);
    }

    public BlobContainerClient GetContainerClient()
    {
        try
        {
            BlobContainerClient containerClient = base.GetContainerClient();
            if (!containerClient.Exists())
            {
                _logger.LogWarning("El contenedor no existe. Se está creando un nuevo contenedor.");
                containerClient.Create();
            }
            return containerClient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el cliente del contenedor");
            throw;
        }
    }

    public async Task<bool> CreateFolderAsync(string folderPath)
    {
        try
        {
            BlobContainerClient containerClient = GetContainerClient();
            string normalizedFolderPath = NormalizeFolderPath(folderPath);

            BlobClient blobClient = containerClient.GetBlobClient($"{normalizedFolderPath}.folder");

            if (!await blobClient.ExistsAsync())
            {
                await blobClient.UploadAsync(new MemoryStream(), true);
                _logger.LogInformation("Carpeta creada: {FolderPath}", normalizedFolderPath);
                return true;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la carpeta {FolderPath}", folderPath);
            return false;
        }
    }

    public async Task<bool> DeleteFolderAsync(string folderPath)
    {
        try
        {
            BlobContainerClient containerClient = GetContainerClient();
            string normalizedFolderPath = NormalizeFolderPath(folderPath);

            var resultSegment = containerClient.GetBlobsAsync(prefix: normalizedFolderPath);
            await foreach (var blobItem in resultSegment)
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                await blobClient.DeleteIfExistsAsync();
            }

            _logger.LogInformation("Carpeta eliminada: {FolderPath}", normalizedFolderPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la carpeta {FolderPath}", folderPath);
            return false;
        }
    }

    public async Task<bool> FolderExistsAsync(string folderPath)
    {
        try
        {
            BlobContainerClient containerClient = GetContainerClient();
            string normalizedFolderPath = NormalizeFolderPath(folderPath);

            var resultSegment = containerClient.GetBlobsAsync(prefix: normalizedFolderPath);
            await foreach (var _ in resultSegment)
            {
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar la existencia de la carpeta {FolderPath}", folderPath);
            return false;
        }
    }

    protected string NormalizeFolderPath(string folderPath)
    {
        return folderPath.EndsWith("/") ? folderPath : $"{folderPath}/";
    }
}