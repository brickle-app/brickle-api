using BricklePlatform.Domain.DTOs;

namespace BricklePlatform.Api.Models;

public class UserActivityLogResponse
{
    public Guid UserId { get; set; }
    public int DaysBack { get; set; }
    public int TotalLogs { get; set; }
    public IEnumerable<UserActivityLogDto> Logs { get; set; } = new List<UserActivityLogDto>();
}