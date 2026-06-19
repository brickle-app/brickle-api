using BricklePlatform.Domain.Common;
using BricklePlatform.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace BricklePlatform.Domain.Entities
{
    public class UserLeasingAgreement
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid LeasingId { get; private set; }
        public decimal AssetValue { get; private set; }
        public decimal UsefulLife { get; private set; }
        public decimal TermTime { get; private set; }
        public string PaymentTerm { get; private set; }
        public AgreementTypeEnum AgreementType { get; private set; }
        public string Currency { get; private set; }
        public string ContractDetails { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public decimal InstallmentRate { get; private set; }
        public decimal InstallmentAmount { get; private set; }
        public decimal ManagementFee { get; private set; }
        public decimal TotalValue { get; private set; }
        public decimal RemainingBalance { get; private set; }
        public string Status { get; private set; }
        public string LeasingCoreAddress { get; private set; }
        public decimal InsurancePercentage { get; private set; }
        public decimal IbrRate { get; private set; }
        public decimal RiskLevel { get; private set; }
        public decimal RiskRate { get; private set; }
        public decimal IVA { get; private set; }
        public decimal ReteIcaPct { get; private set; }
        public decimal ReteFuentePct { get; private set; }
        public decimal BuyerRetentionPercentage { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        [ForeignKey("UserId")]
        public virtual User User { get; private set; }

        [ForeignKey("LeasingId")]
        public virtual Leasing Leasing { get; private set; }

        private UserLeasingAgreement()
        { }

        public static UserLeasingAgreement Create(
            Guid userId,
            Guid leasingId,
            decimal assetValue,
            decimal usefulLife,
            decimal termTime,
            AgreementTypeEnum agreementType,
            string paymentTerm,
            string currency,
            string contractDetails,
            DateTime startDate,
            DateTime endDate,
            decimal installmentRate,
            decimal residualValue,
            decimal managementFee,
            string leasingCoreAddress,
            decimal insurancePercentage,
            decimal ibrrate,
            decimal riskLevel,
            decimal riskRate,
            decimal iva,
            decimal reteIcaPct,
            decimal reteFuentePct,
            decimal buyerRetentionPercentage,
            Leasing leasing
            )
        {
            if (leasing == null)
                throw new DomainException("Leasing cannot be null");

            if (residualValue < 0)
                throw new DomainException("El monto del valor residual del activo debe ser mayor o igual a 0");

            decimal lendedValue = leasing.Price - residualValue;
            double installmentPercentage = (double)installmentRate / 100;
            var monthlyPayment = (((double)leasing.Price * installmentPercentage * Math.Pow((double)(1 + installmentPercentage), (double)termTime)) - ((double)residualValue * installmentPercentage)) / (Math.Pow(1 + installmentPercentage, (double)termTime) - 1);

            return new UserLeasingAgreement
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LeasingId = leasingId,
                AssetValue = assetValue,
                UsefulLife = usefulLife,
                TermTime = termTime,
                PaymentTerm = paymentTerm,
                AgreementType = agreementType,
                Currency = currency,
                ContractDetails = contractDetails,
                StartDate = startDate,
                EndDate = endDate,
                InstallmentRate = installmentRate,
                InstallmentAmount = (decimal)monthlyPayment,
                ManagementFee = managementFee,
                TotalValue = lendedValue,
                RemainingBalance = lendedValue,
                Status = "Active",
                LeasingCoreAddress = leasingCoreAddress,
                InsurancePercentage = insurancePercentage,
                IbrRate = ibrrate,
                RiskLevel = riskLevel,
                RiskRate = riskRate,
                IVA = iva,
                ReteIcaPct = reteIcaPct,
                ReteFuentePct = reteFuentePct,
                BuyerRetentionPercentage = buyerRetentionPercentage,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(decimal remainingBalance, DateTime endDate, string status)
        {
            if (remainingBalance < 0)
                throw new DomainException("El saldo restante no puede ser negativo");

            if (endDate <= DateTime.UtcNow)
                throw new DomainException("La fecha de finalización debe ser mayor a la fecha actual");

            if (string.IsNullOrWhiteSpace(status))
                throw new DomainException("El estado no puede estar vacío");

            if (status.Length > 50)
                throw new DomainException("El estado no puede exceder los 50 caracteres");

            RemainingBalance = remainingBalance;
            EndDate = endDate;
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateRemainingBalance(decimal remainingBalance)
        {
            if (remainingBalance < 0)
                throw new DomainException("El saldo restante no puede ser negativo");

            RemainingBalance = remainingBalance;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Actualiza el saldo restante tras un pago. No valida paymentAmount vs RemainingBalance porque
        /// la cuota mensual incluye principal + intereses; solo una parte va a principal (token holders).
        /// Si el pago excede el saldo restante, se fija en 0 para no bloquear pagos válidos.
        /// </summary>
        public void ProcessPayment(decimal paymentAmount)
        {
            RemainingBalance = Math.Max(0, RemainingBalance - paymentAmount);
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateLeasingCoreAddress(string leasingCoreAddress)
        {
            if (string.IsNullOrWhiteSpace(leasingCoreAddress))
                throw new DomainException("La dirección del contrato de leasing no puede estar vacía");

            LeasingCoreAddress = leasingCoreAddress;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}