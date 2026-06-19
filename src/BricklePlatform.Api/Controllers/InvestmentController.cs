using BricklePlatform.Api.Application.Commands.Investment;
using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Api.Application.Queries.Investment;
using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Controllers
{
    /// <summary>
    /// Controlador responsable de la gestión de inversiones en el sistema.
    /// Implementa operaciones CRUD sobre la entidad inversión.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvestmentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InvestmentController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de inversiones.
        /// </summary>
        /// <param name="mediator">Mediador para el manejo de comandos y consultas CQRS.</param>
        /// <param name="logger">Logger para el registro de eventos y errores.</param>
        public InvestmentController(IMediator mediator, ILogger<InvestmentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva inversión en el sistema.
        /// </summary>
        /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
        /// <param name="request">Datos de la inversión a crear, incluyendo usuario, leasing, cantidad y nombre de bricks.</param>
        /// <returns>
        /// 200 OK: Retorna los datos de la inversión creada exitosamente.
        /// 400 Bad Request: Si los datos de entrada no son válidos.
        /// 500 Internal Server Error: En caso de error interno del servidor.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateInvestmentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateInvestment(
            [FromHeaderModel] HeaderRequestModel header,
            [FromBody] CreateInvestmentDto request)
        {
            try
            {
                _logger.LogInformation("Creando nueva inversión para usuario: {UserId} - CorrelationId: {CorrelationId}",
                    request.UserId, header.CorrelationId);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var command = new CreateInvestmentCommand
                {
                    UserId = request.UserId,
                    LeasingId = request.LeasingId,
                    Amount = request.Amount,
                    BricksCount = request.BricksCount,
                    BricksName = request.BricksName
                };

                var result = await _mediator.Send(command);

                _logger.LogInformation("Inversión creada exitosamente con ID: {InvestmentId} - CorrelationId: {CorrelationId}",
                    result.UserId, header.CorrelationId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear inversión - CorrelationId: {CorrelationId}", header.CorrelationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene una inversión específica por su identificador único.
        /// </summary>
        /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
        /// <param name="id">Identificador único de la inversión a buscar.</param>
        /// <returns>
        /// 200 OK: Retorna los datos completos de la inversión encontrada.
        /// 404 Not Found: Si no existe una inversión con el ID proporcionado.
        /// 500 Internal Server Error: En caso de error interno del servidor.
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InvestmentDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvestment(
            [FromHeaderModel] HeaderRequestModel header,
            Guid id)
        {
            try
            {
                _logger.LogInformation("Obteniendo inversión con ID: {Id} - CorrelationId: {CorrelationId}",
                    id, header.CorrelationId);

                var query = new GetInvestmentByIdQuery(id);
                var investment = await _mediator.Send(query);

                if (investment == null)
                {
                    _logger.LogWarning("Inversión con ID: {Id} no encontrada - CorrelationId: {CorrelationId}",
                        id, header.CorrelationId);
                    return NotFound();
                }

                _logger.LogInformation("Inversión con ID: {Id} obtenida exitosamente - CorrelationId: {CorrelationId}",
                    id, header.CorrelationId);

                return Ok(investment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inversión con ID: {Id} - CorrelationId: {CorrelationId}",
                    id, header.CorrelationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todas las inversiones asociadas a un usuario específico.
        /// </summary>
        /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
        /// <param name="userId">Identificador único del usuario para obtener sus inversiones.</param>
        /// <returns>
        /// 200 OK: Retorna la lista de inversiones del usuario con información completa del leasing.
        /// 500 Internal Server Error: En caso de error interno del servidor.
        /// </returns>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InvestmentDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvestmentsByUserId(
            [FromHeaderModel] HeaderRequestModel header,
            Guid userId)
        {
            try
            {
                _logger.LogInformation("Obteniendo inversiones para el usuario: {UserId} - CorrelationId: {CorrelationId}",
                    userId, header.CorrelationId);

                var query = new GetInvestmentsByUserIdQuery(userId);
                var investments = await _mediator.Send(query);

                _logger.LogInformation("Se obtuvieron {Count} inversiones para el usuario: {UserId} - CorrelationId: {CorrelationId}",
                    investments.Count(), userId, header.CorrelationId);

                return Ok(investments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inversiones para el usuario: {UserId} - CorrelationId: {CorrelationId}",
                    userId, header.CorrelationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todas las inversiones registradas en el sistema.
        /// </summary>
        /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
        /// <returns>
        /// 200 OK: Retorna la lista de todas las inversiones con información completa del leasing y usuario.
        /// 500 Internal Server Error: En caso de error interno del servidor.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InvestmentDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllInvestments([FromHeaderModel] HeaderRequestModel header)
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las inversiones - CorrelationId: {CorrelationId}",
                    header.CorrelationId);

                var query = new GetAllInvestmentsQuery();
                var investments = await _mediator.Send(query);

                _logger.LogInformation("Se obtuvieron {Count} inversiones exitosamente - CorrelationId: {CorrelationId}",
                    investments.Count(), header.CorrelationId);

                return Ok(investments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las inversiones - CorrelationId: {CorrelationId}",
                    header.CorrelationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Reclama la renta de un contrato de arrendamiento específico.
        /// </summary>
        /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
        /// <param name="userLeasingAgreementId">Identificador único del contrato de arrendamiento de usuario.</param>
        /// <param name="request">Datos necesarios para el reclamo de renta, incluyendo token, receptor y firma de permiso.</param>
        /// <returns>
        /// 200 OK: Retorna true si el reclamo de renta fue exitoso.
        /// 400 Bad Request: Si los datos de entrada no son válidos.
        /// 500 Internal Server Error: En caso de error interno del servidor.
        /// </returns>
        [HttpPost("claim-rent/{userId}/{leasingId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClaimRent(
            [FromHeaderModel] HeaderRequestModel header,
            Guid userId,
            Guid leasingId,
            [FromBody] ClaimRentDto request)
        {
            try
            {
                _logger.LogInformation("Reclamando renta para contrato de arrendamiento: {leasingId}, usuario: {userId} - CorrelationId: {CorrelationId}",
                    leasingId, userId, header.CorrelationId);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var command = new ClaimRentCommand(header, userId, leasingId, request);
                var result = await _mediator.Send(command);

                _logger.LogInformation("Renta reclamada exitosamente para contrato: {leasingId}, usuario: {userId} - CorrelationId: {CorrelationId}",
                    leasingId, userId, header.CorrelationId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reclamar renta para contrato: {leasingId}, usuario: {userId} - CorrelationId: {CorrelationId}",
                    leasingId, userId, header.CorrelationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
            }
        }
    }
}