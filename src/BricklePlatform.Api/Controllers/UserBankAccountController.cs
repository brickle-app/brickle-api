using BricklePlatform.Api.Application.Commands.UserBankAccount;
using BricklePlatform.Api.Application.Queries.UserBankAccount;
using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BricklePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserBankAccountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserBankAccountController> _logger;

    public UserBankAccountController(IMediator mediator, ILogger<UserBankAccountController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<UserBankAccountSummaryDto>>> GetUserBankAccounts(
        [FromHeaderModel] HeaderRequestModel header,
        [Required] Guid userId)
    {
        try
        {
            var query = new GetUserBankAccountsQuery(header, userId);
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo cuentas bancarias para usuario: {UserId}", userId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserBankAccountDto>> GetUserBankAccountById(
        [FromHeaderModel] HeaderRequestModel header,
        [Required] Guid id)
    {
        try
        {
            var query = new GetUserBankAccountByIdQuery(header, id);
            
            var result = await _mediator.Send(query);
            
            if (result == null)
                return NotFound(new { message = "Cuenta bancaria no encontrada" });
                
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo cuenta bancaria por ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserBankAccountDto>> CreateUserBankAccount(
        [FromHeaderModel] HeaderRequestModel header,
        [Required][FromBody] CreateUserBankAccountDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new CreateUserBankAccountCommand(header, createDto);
            
            var result = await _mediator.Send(command);
            return CreatedAtAction(
                nameof(GetUserBankAccountById), 
                new { id = result.Id }, 
                result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Datos inválidos para crear cuenta bancaria");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando cuenta bancaria para usuario: {UserId}", createDto.UserId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserBankAccountDto>> UpdateUserBankAccount(
        [FromHeaderModel] HeaderRequestModel header,
        [Required] Guid id,
        [Required][FromBody] UpdateUserBankAccountDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new UpdateUserBankAccountCommand(header, id, updateDto);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Cuenta bancaria no encontrada" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Datos inválidos para actualizar cuenta bancaria: {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando cuenta bancaria: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUserBankAccount(
        [FromHeaderModel] HeaderRequestModel header,
        [Required] Guid id)
    {
        try
        {
            var command = new DeleteUserBankAccountCommand(header, id);
            
            var result = await _mediator.Send(command);
            
            if (!result)
                return NotFound(new { message = "Cuenta bancaria no encontrada" });
                
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando cuenta bancaria: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("user/{userId}/count")]
    public async Task<ActionResult<int>> GetUserBankAccountCount(
        [FromHeaderModel] HeaderRequestModel header,
        [Required] Guid userId)
    {
        try
        {
            var query = new GetUserBankAccountsQuery(header, userId);
            
            var accounts = await _mediator.Send(query);
            return Ok(accounts.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo conteo de cuentas bancarias para usuario: {UserId}", userId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("user/{userId}/active")]
    public async Task<ActionResult<IEnumerable<UserBankAccountSummaryDto>>> GetActiveUserBankAccounts(
        [FromHeaderModel] HeaderRequestModel header,
        [Required] Guid userId)
    {
        try
        {
            var query = new GetUserBankAccountsQuery(header, userId);
            
            var accounts = await _mediator.Send(query);
            // Since the existing entity doesn't have IsActive, return all accounts for now
            var activeAccounts = accounts;
            
            return Ok(activeAccounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo cuentas bancarias activas para usuario: {UserId}", userId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}