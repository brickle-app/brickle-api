using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Domain.DTOs;

public class UserLeasingAgreementInfoDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LeasingId { get; set; }
    public decimal AssetValue { get; set; }
    public decimal UsefulLife { get; set; }
    public decimal TermTime { get; set; }
    public string PaymentTerm { get; set; } = string.Empty;
    public AgreementTypeEnum AgreementType { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string ContractDetails { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal InstallmentRate { get; set; }
    public decimal InstallmentAmount { get; set; }
    public decimal ManagementFee { get; set; }
    public decimal TotalValue { get; set; }
    public decimal RemainingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public string LeasingCoreAddress { get; set; } = string.Empty;
    public decimal InsurancePercentage { get; set; }
    public decimal IbrRate { get; set; }
    public decimal RiskLevel { get; set; }
    public decimal RiskRate { get; set; }
    public decimal IVA { get; set; }
    public decimal ReteIcaPct { get; set; }
    public decimal ReteFuentePct { get; set; }
    public decimal BuyerRetentionPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}