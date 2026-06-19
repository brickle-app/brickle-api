namespace BricklePlatform.Domain.DTOs;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OperationTime { get; set; }
    public string OperationMeasure { get; set; } = string.Empty;
    public string CreditRating { get; set; } = string.Empty;
    public string? LeasingContract { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}