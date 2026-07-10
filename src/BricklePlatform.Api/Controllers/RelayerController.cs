using System.Text.Json;
using BricklePlatform.Domain.DTOs.Relayer;
using BricklePlatform.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RelayerController : ControllerBase
{
    private readonly IRelayerService _relayerService;
    private readonly ILogger<RelayerController> _logger;

    public RelayerController(IRelayerService relayerService, ILogger<RelayerController> logger)
    {
        _relayerService = relayerService;
        _logger = logger;
    }

    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RelayerStatusDto))]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var status = await _relayerService.GetStatusAsync(cancellationToken);
        return Ok(status);
    }

    [HttpPost("sponsor")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RelayerTransactionResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(RelayerTransactionResponseDto))]
    public async Task<IActionResult> Sponsor([FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        var request = ParseRequest(body);
        if (request == null)
        {
            return BadRequest(new RelayerTransactionResponseDto
            {
                Status = false,
                ErrorMessage = "Invalid relayer request body"
            });
        }

        _logger.LogInformation("Relayer sponsor command received: {Command}", request.Command);
        var result = await _relayerService.SponsorAsync(request, cancellationToken);
        return Ok(result);
    }

    private static RelayerSponsorRequestDto? ParseRequest(JsonElement body)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        if (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("params", out _))
        {
            var defenderRequest = JsonSerializer.Deserialize<DefenderStyleRelayerRequestDto>(body.GetRawText(), options);
            return defenderRequest?.Params.FirstOrDefault();
        }

        return JsonSerializer.Deserialize<RelayerSponsorRequestDto>(body.GetRawText(), options);
    }
}
