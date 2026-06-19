using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Constants;
using BricklePlatform.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace BricklePlatform.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    private readonly IBlobStorageRepository _blobStorageRepository;
    private const int MaxFileSize = 5 * 1024 * 1024; // 5MB

    public FileService(
        ILogger<FileService> logger,
        IBlobStorageRepository blobStorageRepository)
    {
        _logger = logger;
        _blobStorageRepository = blobStorageRepository;
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateFileAsync(Stream? fileStream, string fileName)
    {
        try
        {
            if (fileStream == null || fileStream.Length == 0)
                return (false, "El archivo está vacío");

            if (fileStream.Length > MaxFileSize)
                return (false, $"El archivo excede el tamaño máximo permitido de {MaxFileSize / (1024 * 1024)}MB");

            // Validar el tipo de archivo basado en la extensión
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
                return (false, "El archivo no tiene extensión");

            // Validar según el tipo de archivo
            if (BlobConstants.ValidImageExtensions.Contains(extension))
            {
                return (true, null);
            }

            return (false, "Tipo de archivo no soportado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar el archivo {FileName}", fileName);
            return (false, "Error al validar el archivo");
        }
    }

    public async Task<string> UploadFileAsync(string entityType, Guid entityId, string propertyName, Stream fileStream, string fileName)
    {
        try
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            // Convertir el stream a bytes
            using MemoryStream memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            // Subir el archivo usando el repositorio de blob
            return await _blobStorageRepository.UploadFileAsync(
                entityType,
                entityId,
                propertyName,
                fileBytes,
                fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al subir archivo para propiedad {PropertyName} de entidad {EntityType} con ID {EntityId}",
                propertyName, entityType, entityId);
            throw;
        }
    }

    public async Task<string?> GetFileUrlAsync(string entityType, Guid entityId, string propertyName)
    {
        try
        {
            return await _blobStorageRepository.GetLatestFileUrlAsync(
                entityType,
                entityId,
                propertyName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al obtener URL del archivo para propiedad {PropertyName} de entidad {EntityType} con ID {EntityId}",
                propertyName, entityType, entityId);
            throw;
        }
    }
}