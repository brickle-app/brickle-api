using BricklePlatform.Api.Application.Commands.Leasing;
using BricklePlatform.Api.Application.Models;
using BricklePlatform.Api.Application.Queries.Leasing;
using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using BricklePlatform.Api.Validators;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Enums;
using BricklePlatform.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Controllers;

/// <summary>
/// Controlador responsable de la gestión de operaciones de leasing en el sistema.
/// Implementa operaciones CRUD sobre la entidad leasing.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeasingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LeasingController> _logger;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de leasing.
    /// </summary>
    /// <param name="mediator">Mediador para el manejo de comandos y consultas CQRS.</param>
    /// <param name="logger">Logger para el registro de eventos y errores.</param>
    public LeasingController(
        IMediator mediator,
        ILogger<LeasingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los leasings disponibles en el sistema.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="active">Filtro opcional para obtener leasings activos (true), inactivos (false) o todos (null).</param>
    /// <returns>
    /// 200 OK: Retorna la lista de todos los leasings disponibles.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<LeasingDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromHeaderModel] HeaderRequestModel header, [FromQuery] bool? active = null)
    {
        try
        {
            _logger.LogInformation("Obteniendo todos los leasings - CorrelationId: {CorrelationId}", header.CorrelationId);

            GetAllLeasingsQuery query = new GetAllLeasingsQuery(active);
            IEnumerable<LeasingDto> leasings = await _mediator.Send(query);

            _logger.LogInformation("Leasings obtenidos exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return Ok(leasings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener leasings - CorrelationId: {CorrelationId}", header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un leasing específico por su identificador único.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="id">Identificador único del leasing a buscar.</param>
    /// <returns>
    /// 200 OK: Retorna los datos completos del leasing encontrado.
    /// 404 Not Found: Si no existe un leasing con el ID proporcionado.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LeasingDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromHeaderModel] HeaderRequestModel header, Guid id)
    {
        try
        {
            _logger.LogInformation("Obteniendo leasing con ID: {Id} - CorrelationId: {CorrelationId}",
                id, header.CorrelationId);

            GetLeasingByIdQuery query = new GetLeasingByIdQuery(id);
            LeasingDto leasing = await _mediator.Send(query);

            _logger.LogInformation("Leasing obtenido exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return Ok(leasing);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Leasing no encontrado - CorrelationId: {CorrelationId}", header.CorrelationId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene la tabla de amortización para un leasing específico.
    /// </summary>
    /// <param name="header">Información de cabecera con el CorrelationId.</param>
    /// <param name="id">Identificador único del leasing.</param>
    /// <returns>Tabla de amortización detallada mes a mes.</returns>
    [HttpGet("{id}/amortization")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AmortizationTableDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAmortization([FromHeaderModel] HeaderRequestModel header, Guid id)
    {
        try
        {
            _logger.LogInformation("Obteniendo tabla de amortización para leasing con ID: {Id} - CorrelationId: {CorrelationId}",
                id, header.CorrelationId);

            GetLeasingAmortizationQuery query = new GetLeasingAmortizationQuery(id);
            AmortizationTableDto table = await _mediator.Send(query);

            _logger.LogInformation("Tabla de amortización obtenida exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return Ok(table);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Leasing no encontrado para amortización - CorrelationId: {CorrelationId}", header.CorrelationId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tabla de amortización - CorrelationId: {CorrelationId}", header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo leasing en el sistema.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="leasingDto">Datos del leasing a crear, incluyendo nombre, cantidad, precio, tokens disponibles y demás propiedades.</param>
    /// <returns>
    /// 201 Created: Retorna el leasing creado con su ID asignado.
    /// 400 Bad Request: Si los datos proporcionados son inválidos.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(LeasingDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] CreateLeasingDto leasingDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creando nuevo leasing - CorrelationId: {CorrelationId}", header.CorrelationId);

            CreateLeasingCommand command = new CreateLeasingCommand(leasingDto);
            LeasingDto leasing = await _mediator.Send(command);

            LeasingDto createdLeasingDto = new LeasingDto
            {
                Id = leasing.Id,
                Name = leasing.Name,
                Quantity = leasing.Quantity,
                Price = leasing.Price,
                Tokens = leasing.Tokens,
                TokensAvailable = leasing.TokensAvailable,
                PricePerToken = leasing.PricePerToken,
                Description = leasing.Description,
                Type = leasing.Type.ToString(),
                ContractTime = leasing.ContractTime,
                Liquidity = leasing.Liquidity.ToString(),
                CoverImageUrl = leasing.CoverImageUrl,
                MiniatureImageUrl = leasing.MiniatureImageUrl,
                ContractAddress = leasing.ContractAddress,
                TIR = leasing.TIR,
                Active = leasing.Active,
                CreatedAt = leasing.CreatedAt,
                UpdatedAt = leasing.UpdatedAt,
                DeletedAt = leasing.DeletedAt
            };

            _logger.LogInformation("Leasing creado exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return CreatedAtAction(nameof(GetById), new { id = leasing.Id }, createdLeasingDto);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al crear leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza los datos de un leasing existente.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="id">Identificador único del leasing a actualizar.</param>
    /// <param name="leasingDto">Datos actualizados del leasing.</param>
    /// <returns>
    /// 200 OK: Retorna los datos actualizados del leasing.
    /// 400 Bad Request: Si los datos proporcionados son inválidos.
    /// 404 Not Found: Si no existe un leasing con el ID proporcionado.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LeasingDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromHeaderModel] HeaderRequestModel header,
        Guid id,
        [FromBody] UpdateLeasingDto leasingDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Actualizando leasing con ID: {Id} - CorrelationId: {CorrelationId}",
                id, header.CorrelationId);

            UpdateLeasingCommand command = new UpdateLeasingCommand(id, leasingDto);
            LeasingDto leasing = await _mediator.Send(command);

            LeasingDto updatedLeasingDto = new LeasingDto
            {
                Id = leasing.Id,
                Name = leasing.Name,
                Quantity = leasing.Quantity,
                Price = leasing.Price,
                Tokens = leasing.Tokens,
                TokensAvailable = leasing.TokensAvailable,
                PricePerToken = leasing.PricePerToken,
                Description = leasing.Description,
                Type = leasing.Type.ToString(),
                ContractTime = leasing.ContractTime,
                Liquidity = leasing.Liquidity.ToString(),
                CoverImageUrl = leasing.CoverImageUrl,
                MiniatureImageUrl = leasing.MiniatureImageUrl,
                ContractAddress = leasing.ContractAddress,
                TIR = leasing.TIR,
                Active = leasing.Active,
                CreatedAt = leasing.CreatedAt,
                UpdatedAt = leasing.UpdatedAt,
                DeletedAt = leasing.DeletedAt
            };

            _logger.LogInformation("Leasing actualizado exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return Ok(updatedLeasingDto);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Leasing no encontrado - CorrelationId: {CorrelationId}", header.CorrelationId);
            return NotFound(new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina un leasing existente del sistema.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="id">Identificador único del leasing a eliminar.</param>
    /// <returns>
    /// 204 No Content: Si el leasing fue eliminado exitosamente.
    /// 404 Not Found: Si no existe un leasing con el ID proporcionado.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete([FromHeaderModel] HeaderRequestModel header, Guid id)
    {
        try
        {
            _logger.LogInformation("Eliminando leasing con ID: {Id} - CorrelationId: {CorrelationId}",
                id, header.CorrelationId);

            await _mediator.Send(new DeleteLeasingCommand(id));

            _logger.LogInformation("Leasing eliminado exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Leasing no encontrado - CorrelationId: {CorrelationId}", header.CorrelationId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Filtra los leasing por categorías y retorna una lista paginada.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="categories">Lista de categorías separadas por comas (opcional).</param>
    /// <param name="page">Número de página (por defecto: 1).</param>
    /// <param name="limit">Cantidad de registros por página (por defecto: 15).</param>
    /// <param name="active">Filtro opcional para obtener leasings activos (true), inactivos (false) o todos (null).</param>
    /// <returns>
    /// 200 OK: Retorna una lista paginada de leasing filtrados.
    /// 400 Bad Request: Si los parámetros son inválidos.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpGet("filter")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<LeasingDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Filter(
        [FromHeaderModel] HeaderRequestModel header,
        [FromQuery] string? categories = null,
        [FromQuery] int? page = null,
        [FromQuery] int? limit = null,
        [FromQuery] bool? active = null)
    {
        try
        {
            _logger.LogInformation("Filtrando leasing - CorrelationId: {CorrelationId}", header.CorrelationId);

            IEnumerable<LeasingTypeEnum>? categoriesList = null;
            if (!string.IsNullOrWhiteSpace(categories))
            {
                List<string> categoryNames = categories.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(c => c.Trim())
                                            .ToList();

                string[] validCategories = Enum.GetNames(typeof(LeasingTypeEnum));
                if (categoryNames.Any(c => !validCategories.Contains(c)))
                {
                    return BadRequest(new
                    {
                        error = ValidationMessages.LEASINGINVALIDCATEGORIES,
                        validCategories
                    });
                }

                categoriesList = categoryNames.Select(c => Enum.Parse<LeasingTypeEnum>(c));
            }

            FilterLeasingQuery query = new FilterLeasingQuery(
                page ?? 0,
                limit ?? 0,
                categoriesList,
                active
            );

            PaginatedResult<LeasingDto> result = await _mediator.Send(query);

            _logger.LogInformation("Filtrado de leasing completado exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al filtrar leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al filtrar leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene leasings agrupados por una categoría específica.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="groupCategory">Categoría de grupo por la cual filtrar los leasings (latestSold, trending, recommended).</param>
    /// <param name="active">Filtro opcional para obtener leasings activos (true), inactivos (false) o todos (null).</param>
    /// <returns>
    /// 200 OK: Retorna la lista de leasings filtrados por la categoría de grupo especificada.
    /// 400 Bad Request: Si la categoría de grupo especificada no es válida.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpGet("grouped")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<LeasingDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByGroupCategory(
        [FromHeaderModel] HeaderRequestModel header,
        [FromQuery] string groupCategory,
        [FromQuery] bool? active = null)
    {
        try
        {
            _logger.LogInformation("Obteniendo leasings por categoría de grupo: {GroupCategory} - CorrelationId: {CorrelationId}",
                groupCategory, header.CorrelationId);

            if (!Enum.TryParse(groupCategory, true, out LeasingGroupCategoryEnum groupCategoryEnum))
            {
                string[] validCategories = Enum.GetNames(typeof(LeasingGroupCategoryEnum));
                return BadRequest(new
                {
                    error = $"Categoría inválida: {groupCategory}",
                    validGroupCategories = validCategories
                });
            }

            GetLeasingsByGroupCategoryQuery query = new GetLeasingsByGroupCategoryQuery(groupCategoryEnum, active);
            IEnumerable<LeasingDto> leasings = await _mediator.Send(query);

            _logger.LogInformation("Leasings obtenidos exitosamente por categoría de grupo - CorrelationId: {CorrelationId}",
                header.CorrelationId);
            return Ok(leasings);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al obtener leasings por categoría de grupo - CorrelationId: {CorrelationId}",
                header.CorrelationId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener leasings por categoría de grupo - CorrelationId: {CorrelationId}",
                header.CorrelationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
        }
    }
}