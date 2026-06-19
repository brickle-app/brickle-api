using BricklePlatform.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BricklePlatform.Domain.Entities
{
    public class Investment
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid LeasingId { get; private set; }
        public decimal Amount { get; private set; }
        public decimal BricksCount { get; private set; }
        public string BricksName { get; private set; }
        public int PaymentCount { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        [ForeignKey("UserId")]
        public virtual User User { get; private set; }

        [ForeignKey("LeasingId")]
        public virtual Leasing Leasing { get; private set; }

        private Investment()
        { }

        public static Investment Create(
            Guid userId,
            Guid leasingId,
            decimal amount,
            decimal bricksCount,
            string bricksName)
        {
            if (userId == Guid.Empty)
                throw new DomainException("User ID cannot be empty");

            if (leasingId == Guid.Empty)
                throw new DomainException("Leasing ID cannot be empty");

            if (amount <= 0)
                throw new DomainException("Amount must be greater than zero");

            if (bricksCount <= 0)
                throw new DomainException("Bricks count must be greater than zero");

            if (string.IsNullOrWhiteSpace(bricksName))
                throw new DomainException("Bricks name cannot be empty");

            if (bricksName.Length > 200)
                throw new DomainException("Bricks name cannot exceed 200 characters");

            return new Investment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LeasingId = leasingId,
                Amount = amount,
                BricksCount = bricksCount,
                BricksName = bricksName,
                PaymentCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(decimal amount, decimal bricksCount, string bricksName)
        {
            if (amount <= 0)
                throw new DomainException("Amount must be greater than zero");

            if (bricksCount <= 0)
                throw new DomainException("Bricks count must be greater than zero");

            if (string.IsNullOrWhiteSpace(bricksName))
                throw new DomainException("Bricks name cannot be empty");

            if (bricksName.Length > 200)
                throw new DomainException("Bricks name cannot exceed 200 characters");

            Amount = amount;
            BricksCount = bricksCount;
            BricksName = bricksName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddToInvestment(decimal additionalAmount, decimal additionalBricks)
        {
            if (additionalAmount <= 0)
                throw new DomainException("Additional amount must be greater than zero");

            if (additionalBricks <= 0)
                throw new DomainException("Additional bricks count must be greater than zero");

            Amount += additionalAmount;
            BricksCount += additionalBricks;
            UpdatedAt = DateTime.UtcNow;
        }

        public void IncrementPaymentCount()
        {
            PaymentCount++;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Incrementa el conteo de cuotas pagas por la cantidad de cuotas reclamadas en una reclamación acumulada.
        /// Ej: si reclamó cuotas 5 y 6 en una sola transacción, se llama con 2.
        /// </summary>
        public void IncrementPaymentCountBy(int installmentsCount)
        {
            if (installmentsCount <= 0)
                return;
            PaymentCount += installmentsCount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DeductBricks(decimal amount)
        {
            if (amount < 0)
                throw new DomainException("Amount to deduct must be non-negative");

            BricksCount -= amount;
            if (BricksCount < 0) BricksCount = 0;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}