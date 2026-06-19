using BricklePlatform.Api.Application.Commands.UserLeasingAgreement;
using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Api.Application.Queries.UserLeasingAgreement;
using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Controllers;

/// <summary>
/// Controlador responsable de la gestión de acuerdos de leasing de usuarios en el sistema.
/// Implementa operaciones CRUD sobre la entidad UserLeasingAgreement.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserLeasingAgreementController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserLeasingAgreementController> _logger;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de acuerdos de leasing.
    /// </summary>
    /// <param name="mediator">Mediador para el manejo de comandos y consultas CQRS.</param>
    /// <param name="logger">Logger para el registro de eventos y errores.</param>
    public UserLeasingAgreementController(
        IMediator mediator,
        ILogger<UserLeasingAgreementController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Crea un nuevo acuerdo de leasing para un usuario.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="agreement">Datos del acuerdo a crear.</param>
    /// <returns>
    /// 201 Created: Retorna el acuerdo creado con su ID asignado.
    /// 400 Bad Request: Si los datos proporcionados son inválidos.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserLeasingAgreementDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] CreateUserLeasingAgreementDto agreement)
    {
        try
        {
            _logger.LogInformation("Creando nuevo acuerdo de leasing - CorrelationId: {CorrelationId}", header.CorrelationId);

            CreateUserLeasingAgreementCommand command = new CreateUserLeasingAgreementCommand(agreement);
            UserLeasingAgreementDto result = await _mediator.Send(command);

            _logger.LogInformation("Acuerdo de leasing creado exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear acuerdo de leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene un acuerdo de leasing específico por su identificador único.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="id">Identificador único del acuerdo a buscar.</param>
    /// <returns>
    /// 200 OK: Retorna los datos completos del acuerdo encontrado.
    /// 404 Not Found: Si no existe un acuerdo con el ID proporcionado.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserLeasingAgreementDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromHeaderModel] HeaderRequestModel header, Guid id)
    {
        try
        {
            _logger.LogInformation("Obteniendo acuerdo de leasing con ID: {Id} - CorrelationId: {CorrelationId}",
                id, header.CorrelationId);

            GetUserLeasingAgreementByIdQuery query = new GetUserLeasingAgreementByIdQuery(id);
            UserLeasingAgreementDto result = await _mediator.Send(query);

            if (result == null)
            {
                throw new ApplicationException("Acuerdo de leasing no encontrado");
            }

            _logger.LogInformation("Acuerdo de leasing obtenido exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener acuerdo de leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene todos los acuerdos de leasing asociados a un usuario específico.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>
    /// 200 OK: Retorna la lista de acuerdos del usuario.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserLeasingAgreementDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUserId([FromHeaderModel] HeaderRequestModel header, Guid userId)
    {
        try
        {
            _logger.LogInformation("Obteniendo acuerdos de leasing para usuario con ID: {UserId} - CorrelationId: {CorrelationId}",
                userId, header.CorrelationId);

            GetUserLeasingAgreementsByUserIdQuery query = new GetUserLeasingAgreementsByUserIdQuery(userId);
            IEnumerable<UserLeasingAgreementDto> result = await _mediator.Send(query);

            _logger.LogInformation("Acuerdos de leasing obtenidos exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener acuerdos de leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene todos los acuerdos de leasing asociados a un leasing específico.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="leasingId">Identificador único del leasing.</param>
    /// <returns>
    /// 200 OK: Retorna la lista de acuerdos del leasing.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpGet("leasing/{leasingId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserLeasingAgreementDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByLeasingId([FromHeaderModel] HeaderRequestModel header, Guid leasingId)
    {
        try
        {
            _logger.LogInformation("Obteniendo acuerdos de leasing para leasing con ID: {LeasingId} - CorrelationId: {CorrelationId}",
                leasingId, header.CorrelationId);

            GetUserLeasingAgreementsByLeasingIdQuery query = new GetUserLeasingAgreementsByLeasingIdQuery(leasingId);
            UserLeasingAgreementDto result = await _mediator.Send(query);

            _logger.LogInformation("Acuerdos de leasing obtenidos exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener acuerdos de leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }

    /// <summary>
    /// Actualiza un acuerdo de leasing existente.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="id">Identificador único del acuerdo a actualizar.</param>
    /// <param name="agreementDto">Datos actualizados del acuerdo.</param>
    /// <returns>
    /// 200 OK: Retorna el acuerdo actualizado.
    /// 400 Bad Request: Si los datos proporcionados son inválidos.
    /// 404 Not Found: Si no existe un acuerdo con el ID proporcionado.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserLeasingAgreementDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromHeaderModel] HeaderRequestModel header,
        Guid id,
        [FromBody] UpdateUserLeasingAgreementDto agreementDto)
    {
        try
        {
            _logger.LogInformation("Actualizando acuerdo de leasing con ID: {Id} - CorrelationId: {CorrelationId}",
                id, header.CorrelationId);

            UpdateUserLeasingAgreementCommand command = new UpdateUserLeasingAgreementCommand(id, agreementDto);
            UserLeasingAgreementDto result = await _mediator.Send(command);

            _logger.LogInformation("Acuerdo de leasing actualizado exitosamente - CorrelationId: {CorrelationId}", header.CorrelationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar acuerdo de leasing - CorrelationId: {CorrelationId}", header.CorrelationId);
            throw;
        }
    }
}