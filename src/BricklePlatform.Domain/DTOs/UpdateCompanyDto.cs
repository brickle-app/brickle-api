namespace BricklePlatform.Domain.DTOs;

public class UpdateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public int OperationTime { get; set; }
    public string OperationMeasure { get; set; } = string.Empty;
    public string CreditRating { get; set; } = string.Empty;
    public string? LeasingContract { get; set; }
}