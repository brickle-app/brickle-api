using BricklePlatform.Api.Application.Commands.Campaign;
using BricklePlatform.Api.Application.Queries.Campaign;
using BricklePlatform.Api.Application.Queries.Property;
using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CampaignController : ControllerBase
{
  private readonly IMediator _mediator;
  private readonly ILogger<CampaignController> _logger;

  public CampaignController(IMediator mediator, ILogger<CampaignController> logger)
  {
    _mediator = mediator;
    _logger = logger;
  }

  [HttpPost]
  [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CampaignDto))]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> CreateCampaign(
      [FromHeaderModel] HeaderRequestModel header,
      [FromBody] CreateTokenizeAsset createTokenizeAsset)
  {
    try
    {
      var command = new CreateCampaignCommand(header, createTokenizeAsset);
      var result = await _mediator.Send(command);
      return StatusCode(StatusCodes.Status201Created, result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error Creando Campaña - CorrelationId: {CorrelationId}",
          header.CorrelationId);
      throw;
    }
  }

  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CampaignDto>))]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> GetAllCampaigns(
      [FromHeaderModel] HeaderRequestModel header)
  {
    try
    {
      var query = new GetAllCampaignsQuery(header);
      var result = await _mediator.Send(query);
      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error obteniendo todas las campañas - CorrelationId: {CorrelationId}",
          header.CorrelationId);
      throw;
    }
  }

  [HttpGet("{id}")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CampaignDto))]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetCampaign(
      [FromHeaderModel] HeaderRequestModel header,
      Guid id)
  {
    try
    {
      var query = new GetCampaignQuery(header, id);
      var result = await _mediator.Send(query);
      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error obteniendo campaña - CorrelationId: {CorrelationId}",
          header.CorrelationId);
      throw;
    }
  }

  [HttpPost("{leasingId}/commitFunds")]
  [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CampaignDto))]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> CommitFunds(
      [FromHeaderModel] HeaderRequestModel header,
      Guid leasingId,
      [FromBody] BuyAssetDto buyAssetDto)
  {
    try
    {
      var command = new CommitFundsCommand(header, leasingId, buyAssetDto);
      var result = await _mediator.Send(command);
      return StatusCode(StatusCodes.Status201Created, result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error haciendo commit de fondos - CorrelationId: {CorrelationId}",
          header.CorrelationId);
      throw;
    }
  }

  [HttpPost("{campaignId}/finalize")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FinalizeCampaignResponse))]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> FinalizeCampaign(
      [FromHeaderModel] HeaderRequestModel header,
      Guid campaignId,
      [FromBody] FinalizeCampaignRequest request)
  {
    try
    {
      var command = new FinalizeCampaignCommand(header, request.UserId, campaignId, request.BrickleAssumeInsurance);
      var result = await _mediator.Send(command);
      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error finalizando campaña - CorrelationId: {CorrelationId}",
          header.CorrelationId);
      throw;
    }
  }
}

public class FinalizeCampaignRequest
{
  public Guid UserId { get; set; }
  /// <summary>True if Brickle assumes insurance; false if the user/lessor does. Default: false (Brickle does not assume insurance).</summary>
  public bool BrickleAssumeInsurance { get; set; } = false;
}