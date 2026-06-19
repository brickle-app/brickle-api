namespace BricklePlatform.Domain.DTOs;

public class CreateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public int OperationTime { get; set; }
    public string OperationMeasure { get; set; } = string.Empty;
    public string CreditRating { get; set; } = string.Empty;
    public string? LeasingContract { get; set; }
    public Guid UserId { get; set; }
}