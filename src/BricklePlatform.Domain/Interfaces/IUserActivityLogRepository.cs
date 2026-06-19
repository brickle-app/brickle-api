using BricklePlatform.Domain.DTOs;

namespace BricklePlatform.Domain.Interfaces;

public interface IUserActivityLogRepository
{
    Task InsertUserActivityLogAsync(UserActivityLogDto userActivityLogDto);
    Task<IEnumerable<UserActivityLogDto>> GetUserActivityLogsByDateAsync(string partitionKey);
    Task<IEnumerable<UserActivityLogDto>> GetUserActivityLogsByUserIdAsync(Guid userId, int daysBack = 30);
}