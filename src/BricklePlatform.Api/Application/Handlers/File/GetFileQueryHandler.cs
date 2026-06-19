using BricklePlatform.Api.Application.Queries.File;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.File;

public class GetFileQueryHandler : IRequestHandler<GetFileQuery, string>
{
    private readonly ILogger<GetFileQueryHandler> _logger;
    private readonly IFileService _fileService;

    public GetFileQueryHandler(
        ILogger<GetFileQueryHandler> logger,
        IFileService fileService)
    {
        _logger = logger;
        _fileService = fileService;
    }

    public async Task<string> Handle(GetFileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Obteniendo URL del archivo {FileType} para entidad {EntityType} con ID {EntityId} - CorrelationId: {CorrelationId}",
                request.FileType ?? "más reciente", request.EntityType, request.EntityId, request.Header.CorrelationId);

            string? fileUrl = await _fileService.GetFileUrlAsync(
                request.EntityType,
                request.EntityId,
                request.FileType ?? string.Empty);

            if (string.IsNullOrEmpty(fileUrl))
            {
                _logger.LogWarning(
                    "No se encontró el archivo {FileType} para entidad {EntityType} con ID {EntityId} - CorrelationId: {CorrelationId}",
                    request.FileType ?? "más reciente", request.EntityType, request.EntityId, request.Header.CorrelationId);
                return string.Empty;
            }

            return fileUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al procesar la consulta GetFileQuery para entidad {EntityType} con ID {EntityId} - CorrelationId: {CorrelationId}",
                request.EntityType, request.EntityId, request.Header.CorrelationId);
            throw;
        }
    }
}