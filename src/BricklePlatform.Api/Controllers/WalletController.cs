using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BricklePlatform.Domain.DTOs.Wallet;
using BricklePlatform.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletBackupService _walletBackupService;
    private readonly ILogger<WalletController> _logger;

    public WalletController(IWalletBackupService walletBackupService, ILogger<WalletController> logger)
    {
        _walletBackupService = walletBackupService;
        _logger = logger;
    }

    [HttpPost("backup")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WalletBackupResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveBackup([FromBody] WalletBackupRequestDto request)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { error = "Authenticated user id is missing" });

        try
        {
            var response = await _walletBackupService.SaveAsync(userId, request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid wallet backup payload for user {UserId}", userId);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("backup/upgrade")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WalletBackupResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpgradeBackup([FromBody] WalletBackupRequestDto request)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { error = "Authenticated user id is missing" });

        try
        {
            var response = await _walletBackupService.UpgradeActiveWalletAsync(userId, request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid wallet upgrade payload for user {UserId}", userId);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("backup")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WalletBackupResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBackup()
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { error = "Authenticated user id is missing" });

        try
        {
            var response = await _walletBackupService.GetAsync(userId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogInformation(ex, "Wallet backup not found for user {UserId}", userId);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid wallet backup state for user {UserId}", userId);
            return BadRequest(new { error = ex.Message });
        }
    }

    private bool TryGetAuthenticatedUserId(out Guid userId)
    {
        var value = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out userId);
    }
}
