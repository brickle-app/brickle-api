using System;
using BricklePlatform.Domain.Enums;

namespace BricklePlatform.Api.Application.Dtos
{
    public class UserLeasingAgreementDto
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
        public required string LeasingCoreAddress { get; set; }
        public string? BaseToken { get; set; }
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

    public class CreateUserLeasingAgreementDto
    {
        public Guid UserId { get; set; }
        public Guid LeasingId { get; set; }
        public required decimal AssetValue { get; set; }
        public decimal UsefulLife { get; set; }
        public decimal TermTime { get; set; }
        public required string PaymentTerm { get; set; }
        public AgreementTypeEnum AgreementType { get; set; }
        public required string Currency { get; set; }
        public required string ContractDetails { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal InstallmentRate { get; set; }
        public decimal ResidualValue { get; set; }
        public decimal? LeasingTokenPrice { get; set; }
        public decimal? FinalPaymentAmount { get; set; }
        public decimal ManagementFee { get; set; }
        public required string LeasingCoreAddress { get; set; }
        public decimal InsurancePercentage { get; set; }
        public decimal IbrRate { get; set; }
        public int RiskLevel { get; set; }
        public decimal RiskRate { get; set; }
        public decimal IVA { get; set; }
        public decimal BuyerRetentionPercentage { get; set; }
    }
}