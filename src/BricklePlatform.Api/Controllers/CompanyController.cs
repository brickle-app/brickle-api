using BricklePlatform.Api.Application.Commands.Company;
using BricklePlatform.Api.Application.Queries.Company;
using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Controllers;

/// <summary>
/// Controlador responsable de la gestión de empresas en el sistema.
/// Implementa operaciones CRUD sobre la entidad empresa.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly ILogger<CompanyController> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de empresas.
    /// </summary>
    /// <param name="logger">Logger para el registro de eventos y errores.</param>
    /// <param name="mediator">Mediador para el manejo de comandos y consultas CQRS.</param>
    public CompanyController(
        ILogger<CompanyController> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Crea una nueva empresa en el sistema.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="createCompany">Datos de la empresa a crear.</param>
    /// <returns>Los datos de la empresa creada.</returns>
    /// <response code="200">Empresa creada exitosamente.</response>
    /// <response code="400">Los datos de entrada no son válidos.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CompanyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CompanyDto>> CreateCompany(
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] CreateCompanyDto createCompany)
    {
        try
        {
            var command = new CreateCompanyCommand(header, createCompany);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company for user: {UserId}. CorrelationId: {CorrelationId}",
                createCompany.UserId, header.CorrelationId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza una empresa existente.
    /// </summary>
    /// <param name="id">ID de la empresa a actualizar.</param>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="updateCompany">Datos actualizados de la empresa.</param>
    /// <returns>Los datos actualizados de la empresa.</returns>
    /// <response code="200">Empresa actualizada exitosamente.</response>
    /// <response code="400">Los datos de entrada no son válidos.</response>
    /// <response code="404">Empresa no encontrada.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpPut("{id}")]
    public async Task<ActionResult<CompanyDto>> UpdateCompany(
        Guid id,
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] UpdateCompanyDto updateCompany)
    {
        try
        {
            var command = new UpdateCompanyCommand(header, id, updateCompany);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company with ID: {CompanyId}. CorrelationId: {CorrelationId}",
                id, header.CorrelationId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Elimina una empresa del sistema.
    /// </summary>
    /// <param name="id">ID de la empresa a eliminar.</param>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <returns>Confirmación de eliminación.</returns>
    /// <response code="200">Empresa eliminada exitosamente.</response>
    /// <response code="404">Empresa no encontrada.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCompany(
        Guid id,
        [FromHeaderModel] HeaderRequestModel header)
    {
        try
        {
            var command = new DeleteCompanyCommand(header, id);
            await _mediator.Send(command);
            return Ok(new { Message = "Empresa eliminada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company with ID: {CompanyId}. CorrelationId: {CorrelationId}",
                id, header.CorrelationId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene una empresa por su ID.
    /// </summary>
    /// <param name="id">ID de la empresa a obtener.</param>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <returns>Los datos de la empresa solicitada.</returns>
    /// <response code="200">Empresa encontrada exitosamente.</response>
    /// <response code="404">Empresa no encontrada.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> GetCompanyById(
        Guid id,
        [FromHeaderModel] HeaderRequestModel header)
    {
        try
        {
            var query = new GetCompanyByIdQuery(header, id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { Message = "Empresa no encontrada" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company with ID: {CompanyId}. CorrelationId: {CorrelationId}",
                id, header.CorrelationId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene todas las empresas del sistema.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <returns>Lista de todas las empresas.</returns>
    /// <response code="200">Lista de empresas obtenida exitosamente.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetAllCompanies(
        [FromHeaderModel] HeaderRequestModel header)
    {
        try
        {
            var query = new GetAllCompaniesQuery(header);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all companies. CorrelationId: {CorrelationId}",
                header.CorrelationId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene las empresas asociadas a un usuario específico.
    /// </summary>
    /// <param name="userId">ID del usuario para buscar sus empresas.</param>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <returns>Lista de empresas del usuario.</returns>
    /// <response code="200">Empresas del usuario encontradas.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompaniesByUserId(
        Guid userId,
        [FromHeaderModel] HeaderRequestModel header)
    {
        try
        {
            var query = new GetCompaniesByUserIdQuery(header, userId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting companies for user ID: {UserId}. CorrelationId: {CorrelationId}",
                userId, header.CorrelationId);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}