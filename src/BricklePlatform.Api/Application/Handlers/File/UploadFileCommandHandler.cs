using BricklePlatform.Api.Application.Commands.File;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Domain.ValueObjects;
using BricklePlatform.Infrastructure.Services;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.File;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, string>
{
    private readonly ILogger<UploadFileCommandHandler> _logger;
    private readonly IFileService _fileService;
    private readonly IServiceProvider _serviceProvider;

    public UploadFileCommandHandler(
        ILogger<UploadFileCommandHandler> logger,
        IFileService fileService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _fileService = fileService;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            IFormFile file = request.Body.File;
            Guid entityId = request.Body.EntityId;

            // Extraer información del nombre del archivo
            FileNameInfo fileNameInfo = FileNameInfo.Create(file.FileName);

            _logger.LogInformation(
                "Procesando archivo para entidad {EntityType} con ID {EntityId} y propiedad {PropertyName}",
                fileNameInfo.EntityType, entityId, fileNameInfo.PropertyName);

            // Validar el archivo
            using Stream stream = file.OpenReadStream();
            (bool isValid, string? errorMessage) = await _fileService.ValidateFileAsync(stream, file.FileName);

            if (!isValid)
            {
                _logger.LogWarning(
                    "Archivo inválido para entidad {EntityType} con ID {EntityId} - Error: {ErrorMessage}",
                    fileNameInfo.EntityType, entityId, errorMessage);
                throw new InvalidOperationException(errorMessage ?? "Error de validación no especificado");
            }

            // Subir el archivo
            string fileUrl = await _fileService.UploadFileAsync(
                fileNameInfo.EntityType,
                entityId,
                fileNameInfo.PropertyName,
                stream,
                file.FileName);

            // Actualizar la URL en la entidad correspondiente
            IEntityFileUpdater fileUpdater = GetFileUpdater(fileNameInfo.EntityType);
            await fileUpdater.UpdateEntityFileUrlAsync(entityId, fileUrl, fileNameInfo.PropertyName);

            return fileUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al procesar archivo para entidad con ID {EntityId}",
                request.Body.EntityId);
            throw;
        }
    }

    private IEntityFileUpdater GetFileUpdater(string entityType)
    {
        return entityType.ToUpperInvariant() switch
        {
            "LEASING" => _serviceProvider.GetRequiredService<LeasingFileUpdater>(),
            "USER" => _serviceProvider.GetRequiredService<UserFileUpdater>(),
            "PAYMENT" => _serviceProvider.GetRequiredService<PaymentFileUpdater>(),
            _ => throw new InvalidOperationException($"No se encontró un actualizador de archivos para la entidad {entityType}")
        };
    }
}