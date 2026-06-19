using BricklePlatform.Api.Application.Commands.File;
using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Api.Application.Queries.File;
using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Controllers;

/// <summary>
/// Controlador responsable de la gestión de archivos en el sistema.
/// Proporciona endpoints para la carga, almacenamiento y recuperación de archivos asociados a diferentes entidades del sistema.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FileController> _logger;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de archivos.
    /// </summary>
    /// <param name="mediator">Mediador para el manejo de comandos y consultas CQRS.</param>
    /// <param name="logger">Logger para el registro de eventos y errores.</param>
    public FileController(
        IMediator mediator,
        ILogger<FileController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Sube un archivo al sistema y lo asocia a una entidad específica.
    /// El nombre del archivo debe seguir el formato: {Entity}.{PropertyName}.{Extension} Ejemplo: Leasing.Miniature.jpg
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="request">Datos de la solicitud que incluyen el archivo a subir y el ID de la entidad asociada.</param>
    /// <returns>
    /// 200 OK: Retorna la URL del archivo subido.
    /// 400 Bad Request: Si el archivo no es válido o el formato del nombre es incorrecto.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadFileAsync([FromHeaderModel] HeaderRequestModel header, [FromForm] UploadFileRequestDto request)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { error = "No se ha proporcionado un archivo válido" });
            }

            FileNameInfo fileNameInfo;
            try
            {
                fileNameInfo = FileNameInfo.Create(request.File.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Formato de nombre de archivo inválido: {FileName}", request.File.FileName);
                return BadRequest(new { error = $"El nombre del archivo debe seguir el formato {{Entity}}.{{PropertyName}}.{{Extension}}. Detalle: {ex.Message}" });
            }

            _logger.LogInformation(
                "Subiendo archivo para entidad {EntityType} con ID: {EntityId} y propiedad {PropertyName} - CorrelationId: {CorrelationId}",
                fileNameInfo.EntityType, request.EntityId, fileNameInfo.PropertyName, header.CorrelationId);

            UploadFileCommand command = new UploadFileCommand(header, request);
            string result = await _mediator.Send(command);

            return Ok(new FileResponseDto { FileUrl = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al subir archivo para entidad con ID {EntityId} - CorrelationId: {CorrelationId}",
                request.EntityId, header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene la URL de un archivo específico asociado a una entidad.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="entityType">Tipo de entidad a la que está asociado el archivo (User, Leasing, etc.).</param>
    /// <param name="entityId">Identificador único de la entidad.</param>
    /// <param name="fileType">Tipo específico del archivo (profile, cover, miniature, etc.). Opcional.</param>
    /// <returns>
    /// 200 OK: Retorna la URL del archivo solicitado.
    /// 404 Not Found: Si el archivo no existe.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpGet("{entityType}/{entityId}/{fileType?}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFileAsync(
        [FromHeaderModel] HeaderRequestModel header,
        string entityType,
        Guid entityId,
        string? fileType = null)
    {
        try
        {
            _logger.LogInformation(
                "Obteniendo archivo {FileType} para entidad {EntityType} con ID: {EntityId} - CorrelationId: {CorrelationId}",
                fileType ?? "más reciente", entityType, entityId, header.CorrelationId);

            GetFileQuery query = new GetFileQuery(header, entityType, entityId, fileType);
            string result = await _mediator.Send(query);

            if (string.IsNullOrEmpty(result))
            {
                return NotFound(new { error = "Archivo no encontrado" });
            }

            return Ok(new FileResponseDto { FileUrl = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al obtener archivo para entidad {EntityType} con ID {EntityId} - CorrelationId: {CorrelationId}",
                entityType, entityId, header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error interno del servidor" });
        }
    }
}