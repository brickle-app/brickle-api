using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace BricklePlatform.Infrastructure.Services;

public interface IUserActivityLogService
{
    Task LogUserActivityAsync(UserActivityLogDto userActivityLogDto);
    Task<IEnumerable<UserActivityLogDto>> GetUserActivityLogsAsync(Guid userId, int daysBack = 30, Guid? leasingId = null, string? type = null, string? status = null);
}

public class UserActivityLogService : IUserActivityLogService
{
    private readonly ILogger<UserActivityLogService> _logger;
    private readonly IUserActivityLogRepository _userActivityLogRepository;
    private readonly IUserService _user;
    private readonly INotificationService _notificationService;
    private const string ADMIN_EXPO_TOKEN = "ExponentPushToken[QwP8m8HZJ3kvzsQURzJE_M]";

    public UserActivityLogService(
        ILogger<UserActivityLogService> logger,
        IUserActivityLogRepository userActivityLogRepository,
        INotificationService notificationService,
        IUserService userService)
    {
        _logger = logger;
        _userActivityLogRepository = userActivityLogRepository;
        _notificationService = notificationService;
        _user = userService;
    }

    public async Task LogUserActivityAsync(UserActivityLogDto userActivityLogDto)
    {
        try
        {
            await _userActivityLogRepository.InsertUserActivityLogAsync(userActivityLogDto);

            _logger.LogInformation(
                "User activity logged successfully for user {UserId}, type {Type}",
                userActivityLogDto.UserId, userActivityLogDto.Type);

            // Send admin notification for RECHARGE activities
            if (userActivityLogDto.Type.Equals("RECHARGE", StringComparison.OrdinalIgnoreCase))
            {
                await SendAdminRechargeNotificationAsync(userActivityLogDto);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error logging user activity for user {UserId}, type {Type}",
                userActivityLogDto.UserId, userActivityLogDto.Type);
            throw;
        }
    }

    private async Task SendAdminRechargeNotificationAsync(UserActivityLogDto userActivityLogDto)
    {
        try
        {
            var notificationData = new
            {
                userId = userActivityLogDto.UserId.ToString(),
                type = userActivityLogDto.Type,
                amount = userActivityLogDto.TxAmount,
                receipt = userActivityLogDto.Receipt,
                reference = userActivityLogDto.Reference,
                timestamp = DateTime.UtcNow
            };

            User user = await _user.GetUserAsync(userActivityLogDto.UserId);

            if (user == null)
            {
                _logger.LogWarning("User not found for UserId: {UserId}", userActivityLogDto.UserId);
                return;
            }

            var data = new Dictionary<string, object>(notificationData.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(notificationData)))
            {
                ["category"] = "MOVEMENT"
            };

            await _notificationService.SendNotificationAsync(
                recipientId: ADMIN_EXPO_TOKEN,
                title: "Recarga pendiente",
                body: $"Por valor de: ${userActivityLogDto.TxAmount:F2} al usuario: ${user.Email}",
                data: data
            );

            _logger.LogInformation(
                "Admin notification sent for RECHARGE activity. UserId: {UserId}, Amount: {Amount}",
                userActivityLogDto.UserId, userActivityLogDto.TxAmount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send admin notification for RECHARGE activity. UserId: {UserId}",
                userActivityLogDto.UserId);
            // Don't throw - notification failure shouldn't break the main flow
        }
    }

    public async Task<IEnumerable<UserActivityLogDto>> GetUserActivityLogsAsync(Guid userId, int daysBack = 30, Guid? leasingId = null, string? type = null, string? status = null)
    {
        try
        {
            var logs = await _userActivityLogRepository.GetUserActivityLogsByUserIdAsync(userId, daysBack);

            if (leasingId.HasValue)
            {
                logs = logs.Where(log => log.LeasingId == leasingId.Value);
            }

            if (!string.IsNullOrEmpty(type))
            {
                logs = logs.Where(log => log.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(status))
            {
                logs = logs.Where(log => log.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activity logs for user {UserId}", userId);
            throw;
        }
    }

}