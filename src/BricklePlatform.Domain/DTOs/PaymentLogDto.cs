namespace BricklePlatform.Domain.DTOs;

public class PaymentLogDto
{
    public Guid UserLeasingAgreementId { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal RemainingBalance { get; set; }
    public string LeasingContractAddress { get; set; } = string.Empty;
    public string UserWallet { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public bool Status { get; set; }
}