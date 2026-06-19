using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserActivityLogController : ControllerBase
{
    private readonly IUserActivityLogService _userActivityLogService;
    private readonly ILogger<UserActivityLogController> _logger;

    public UserActivityLogController(
        IUserActivityLogService userActivityLogService,
        ILogger<UserActivityLogController> logger)
    {
        _userActivityLogService = userActivityLogService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogUserActivity(
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] UserActivityLogDto userActivityLogDto)
    {
        try
        {
            if (userActivityLogDto.UserId == Guid.Empty)
            {
                return BadRequest("UserId is required");
            }

            if (string.IsNullOrEmpty(userActivityLogDto.Type))
            {
                return BadRequest("Type is required");
            }

            await _userActivityLogService.LogUserActivityAsync(userActivityLogDto);

            _logger.LogInformation(
                "User activity logged successfully for user {UserId}, type {Type} - CorrelationId: {CorrelationId}",
                userActivityLogDto.UserId, userActivityLogDto.Type, header.CorrelationId);

            return Ok(new { message = "User activity logged successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error logging user activity for user {UserId} - CorrelationId: {CorrelationId}",
                userActivityLogDto?.UserId, header.CorrelationId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserActivityLogResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserActivityLogs(
        [FromHeaderModel] HeaderRequestModel header,
        Guid userId,
        [FromQuery] int daysBack = 30,
        [FromQuery] Guid? leasingId = null,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("UserId is required");
            }

            if (daysBack <= 0 || daysBack > 365)
            {
                return BadRequest("DaysBack must be between 1 and 365");
            }

            var logs = await _userActivityLogService.GetUserActivityLogsAsync(userId, daysBack, leasingId, type, status);

            _logger.LogInformation(
                "Retrieved {LogCount} activity logs for user {UserId} over {DaysBack} days - CorrelationId: {CorrelationId}",
                logs.Count(), userId, daysBack, header.CorrelationId);

            var response = new UserActivityLogResponse
            {
                UserId = userId,
                DaysBack = daysBack,
                TotalLogs = logs.Count(),
                Logs = logs
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving user activity logs for user {UserId} - CorrelationId: {CorrelationId}",
                userId, header.CorrelationId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

}