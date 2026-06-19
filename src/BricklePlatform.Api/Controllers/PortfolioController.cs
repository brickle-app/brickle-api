using BricklePlatform.Api.Application.Queries.Portfolio;
using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Controllers
{
    /// <summary>
    /// Controlador responsable de la gestión de portfolios y análisis de inversiones.
    /// Proporciona endpoints para obtener resúmenes y análisis de cartera.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PortfolioController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PortfolioController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de portfolio.
        /// </summary>
        /// <param name="mediator">Mediador para el manejo de comandos y consultas CQRS.</param>
        /// <param name="logger">Logger para el registro de eventos y errores.</param>
        public PortfolioController(IMediator mediator, ILogger<PortfolioController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el resumen de portfolio de un usuario para un rango de fechas específico.
        /// Incluye valor actual, ROI y gráfico mensual.
        /// </summary>
        /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
        /// <param name="userId">Identificador único del usuario para obtener su portfolio.</param>
        /// <param name="from">Fecha de inicio del rango en formato YYYY-MM (requerido).</param>
        /// <param name="to">Fecha de fin del rango en formato YYYY-MM (requerido).</param>
        /// <returns>
        /// 200 OK: Retorna el resumen completo del portfolio con datos históricos.
        /// 400 Bad Request: Si los parámetros de fecha son inválidos o están fuera de rango.
        /// 500 Internal Server Error: En caso de error interno del servidor.
        /// </returns>
        [HttpGet("overview")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PortfolioOverviewDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOverview(
            [FromHeaderModel] HeaderRequestModel header,
            [FromQuery] Guid userId,
            [FromQuery] string from,
            [FromQuery] string to)
        {
            try
            {
                _logger.LogInformation("Obteniendo overview de portfolio para usuario: {UserId}, rango: {From} - {To} - CorrelationId: {CorrelationId}",
                    userId, from, to, header.CorrelationId);

                // Validar parámetros de entrada
                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("UserId inválido proporcionado - CorrelationId: {CorrelationId}", header.CorrelationId);
                    return BadRequest(new { error = "UserId es requerido y debe ser válido" });
                }

                if (!DateOnly.TryParseExact(from, "yyyy-MM", out var fromDate))
                {
                    _logger.LogWarning("Formato de fecha 'from' inválido: {From} - CorrelationId: {CorrelationId}", from, header.CorrelationId);
                    return BadRequest(new { error = "El parámetro 'from' debe tener formato YYYY-MM" });
                }

                if (!DateOnly.TryParseExact(to, "yyyy-MM", out var toDate))
                {
                    _logger.LogWarning("Formato de fecha 'to' inválido: {To} - CorrelationId: {CorrelationId}", to, header.CorrelationId);
                    return BadRequest(new { error = "El parámetro 'to' debe tener formato YYYY-MM" });
                }

                if (fromDate > toDate)
                {
                    _logger.LogWarning("Rango de fechas inválido: from ({From}) > to ({To}) - CorrelationId: {CorrelationId}", 
                        from, to, header.CorrelationId);
                    return BadRequest(new { error = "La fecha 'from' no puede ser posterior a 'to'" });
                }

                var query = new GetPortfolioOverviewQuery(userId, fromDate, toDate);

                var result = await _mediator.Send(query);

                _logger.LogInformation("Overview de portfolio obtenido exitosamente para usuario: {UserId} - CorrelationId: {CorrelationId}",
                    userId, header.CorrelationId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener overview de portfolio para usuario: {UserId} - CorrelationId: {CorrelationId}",
                    userId, header.CorrelationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Calcula proyecciones futuras del portfolio basadas en el valor actual y retorno esperado.
        /// </summary>
        /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
        /// <param name="userId">Identificador único del usuario para obtener proyecciones de su portfolio.</param>
        /// <param name="currentValue">Valor actual del portfolio desde el cual proyectar.</param>
        /// <param name="projectionMonths">Número de meses para proyección futura (requerido, mayor a 0).</param>
        /// <param name="expectedAnnualReturn">Tasa anual esperada para proyecciones (requerido, ej: 0.07 para 7%).</param>
        /// <param name="startDate">Fecha de inicio para las proyecciones en formato YYYY-MM (requerido).</param>
        /// <returns>
        /// 200 OK: Retorna las proyecciones del portfolio por mes.
        /// 400 Bad Request: Si los parámetros son inválidos.
        /// 500 Internal Server Error: En caso de error interno del servidor.
        /// </returns>
        [HttpGet("projections")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProjectionPointDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProjections(
            [FromHeaderModel] HeaderRequestModel header,
            [FromQuery] Guid userId,
            [FromQuery] decimal currentValue,
            [FromQuery] int projectionMonths,
            [FromQuery] decimal expectedAnnualReturn,
            [FromQuery] string startDate,
            [FromQuery] decimal? investedPrincipal = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo proyecciones de portfolio para usuario: {UserId}, valor: {CurrentValue}, meses: {ProjectionMonths} - CorrelationId: {CorrelationId}",
                    userId, currentValue, projectionMonths, header.CorrelationId);

                // Validar parámetros de entrada
                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("UserId inválido proporcionado - CorrelationId: {CorrelationId}", header.CorrelationId);
                    return BadRequest(new { error = "UserId es requerido y debe ser válido" });
                }

                if (currentValue <= 0)
                {
                    _logger.LogWarning("Valor actual inválido: {CurrentValue} - CorrelationId: {CorrelationId}", currentValue, header.CorrelationId);
                    return BadRequest(new { error = "El valor actual del portfolio debe ser mayor a 0" });
                }

                if (projectionMonths <= 0)
                {
                    _logger.LogWarning("Número de meses de proyección inválido: {ProjectionMonths} - CorrelationId: {CorrelationId}", 
                        projectionMonths, header.CorrelationId);
                    return BadRequest(new { error = "El número de meses de proyección debe ser mayor a 0" });
                }

                if (expectedAnnualReturn < -1 || expectedAnnualReturn > 10)
                {
                    _logger.LogWarning("Tasa anual esperada fuera de rango: {ExpectedAnnualReturn} - CorrelationId: {CorrelationId}", 
                        expectedAnnualReturn, header.CorrelationId);
                    return BadRequest(new { error = "La tasa anual esperada debe estar entre -100% y 1000% (-1.0 a 10.0)" });
                }

                if (!DateOnly.TryParseExact(startDate, "yyyy-MM", out var startDateParsed))
                {
                    _logger.LogWarning("Formato de fecha de inicio inválido: {StartDate} - CorrelationId: {CorrelationId}", startDate, header.CorrelationId);
                    return BadRequest(new { error = "El parámetro 'startDate' debe tener formato YYYY-MM" });
                }

                var principal = investedPrincipal is > 0 ? investedPrincipal.Value : currentValue;

                var query = new GetPortfolioProjectionsQuery(userId, currentValue, projectionMonths, expectedAnnualReturn, startDateParsed, principal);

                var result = await _mediator.Send(query);

                _logger.LogInformation("Proyecciones de portfolio obtenidas exitosamente para usuario: {UserId} - CorrelationId: {CorrelationId}",
                    userId, header.CorrelationId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener proyecciones de portfolio para usuario: {UserId} - CorrelationId: {CorrelationId}",
                    userId, header.CorrelationId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor" });
            }
        }
    }
}