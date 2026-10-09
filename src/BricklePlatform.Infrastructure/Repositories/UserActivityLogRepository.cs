using Azure.Data.Tables;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Entities;
using BricklePlatform.Infrastructure.Services.Base;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace BricklePlatform.Infrastructure.Repositories;

public class UserActivityLogRepository : AzureTableStorageBase<UserActivityLogEntity>, IUserActivityLogRepository
{
    private readonly ILogger<UserActivityLogRepository> _logger;
    private readonly string tableName;
    private const int MaxConcurrentPartitionQueries = 16;

    public UserActivityLogRepository(
        IOptions<InfrastructureSettings> settings,
        ILogger<UserActivityLogRepository> logger)
        : base(settings.Value.AzureSettings.ConnectionString, settings.Value.AzureSettings.UserActivityLogsTableName)
    {
        _logger = logger;
        tableName = settings.Value.AzureSettings.UserActivityLogsTableName;
        
        // Ensure table exists
        _ = Task.Run(async () => await EnsureTableExistsAsync(settings.Value.AzureSettings.ConnectionString, tableName));
    }

    public async Task InsertUserActivityLogAsync(UserActivityLogDto userActivityLogDto)
    {
        try
        {
            var eventUtc = ResolveEventUtc(userActivityLogDto.Timestamp);
            string currentDateString = eventUtc.ToString("ddMMyyyy");
            string partitionKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(currentDateString));

            UserActivityLogEntity userActivityLogEntity = new()
            {
                PartitionKey = partitionKey,
                RowKey = Guid.NewGuid().ToString(),
                UserId = userActivityLogDto.UserId.ToString(),
                Type = userActivityLogDto.Type,
                TxAmount = (double)userActivityLogDto.TxAmount,
                Status = userActivityLogDto.Status,
                Receipt = userActivityLogDto.Receipt,
                Hash = userActivityLogDto.Hash,
                Reference = userActivityLogDto.Reference,
                LeasingId = userActivityLogDto.LeasingId?.ToString() ?? string.Empty,
                EventTime = eventUtc
            };

            await InsertAsync(userActivityLogEntity);
            
            _logger.LogInformation(
                "User activity log inserted successfully for user {UserId}, type {Type}", 
                userActivityLogDto.UserId, userActivityLogDto.Type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error insertando registro de actividad de usuario en la tabla {UserActivityLogsTableName}", tableName);
            throw;
        }
    }

    public async Task<IEnumerable<UserActivityLogDto>> GetUserActivityLogsByDateAsync(string partitionKey)
    {
        try
        {
            var entities = await QueryAllAsync(partitionKey);
            var result = entities.Select(MapToDto).OrderByDescending(x => x.Timestamp);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activity logs for partition {PartitionKey}", partitionKey);
            throw;
        }
    }

    public async Task<IEnumerable<UserActivityLogDto>> GetUserActivityLogsByUserIdAsync(Guid userId, int daysBack = 30)
    {
        try
        {
            DateTime endDate = DateTime.UtcNow;
            DateTime startDate = endDate.AddDays(-daysBack);

            // One partition per day: query them concurrently and let the server filter by user,
            // instead of downloading every user's logs for every day sequentially.
            var days = new List<DateTime>();
            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                days.Add(date);

            using var throttle = new SemaphoreSlim(MaxConcurrentPartitionQueries);
            var perDay = await Task.WhenAll(days.Select(async date =>
            {
                await throttle.WaitAsync();
                try
                {
                    string partitionKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(date.ToString("ddMMyyyy")));
                    var entities = await QueryByFilterAsync(
                        $"PartitionKey eq '{partitionKey}' and UserId eq '{userId}'");
                    return entities.Select(MapToDto).ToList();
                }
                finally
                {
                    throttle.Release();
                }
            }));

            return perDay.SelectMany(logs => logs).OrderByDescending(log => log.Timestamp); // Order by most recent first
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activity logs for user {UserId}", userId);
            throw;
        }
    }

    private static UserActivityLogDto MapToDto(UserActivityLogEntity entity) => new()
    {
        UserId = Guid.Parse(entity.UserId),
        Type = entity.Type,
        TxAmount = (decimal)entity.TxAmount,
        Status = entity.Status,
        Receipt = entity.Receipt,
        Hash = entity.Hash,
        Reference = entity.Reference,
        LeasingId = string.IsNullOrEmpty(entity.LeasingId) ? null : Guid.Parse(entity.LeasingId),
        Timestamp = ResolveLogTimestamp(entity)
    };

    private static DateTime ResolveEventUtc(DateTime dtoTimestamp)
    {
        if (dtoTimestamp == default || dtoTimestamp.Year < 2000)
            return DateTime.UtcNow;
        return dtoTimestamp.Kind switch
        {
            DateTimeKind.Utc => dtoTimestamp,
            DateTimeKind.Local => dtoTimestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dtoTimestamp, DateTimeKind.Utc)
        };
    }

    private static DateTime ResolveLogTimestamp(UserActivityLogEntity entity)
    {
        if (entity.EventTime.HasValue && entity.EventTime.Value.Year >= 2000)
            return DateTime.SpecifyKind(entity.EventTime.Value, DateTimeKind.Utc);
        return entity.Timestamp?.UtcDateTime ?? DateTime.UtcNow;
    }

    private static async Task EnsureTableExistsAsync(string connectionString, string tableName)
    {
        try
        {
            TableServiceClient tableServiceClient = new TableServiceClient(connectionString);
            await tableServiceClient.CreateTableIfNotExistsAsync(tableName);
        }
        catch (Exception)
        {
            // Silently ignore table creation errors as table might already exist
            // or there might be permission issues that don't affect normal operations
        }
    }
}