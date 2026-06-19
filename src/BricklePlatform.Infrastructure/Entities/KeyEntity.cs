namespace BricklePlatform.Infrastructure.Entities;

public class ApiKey
{
    public int Id { get; set; }
    public string Application { get; set; }
    public string Key { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}