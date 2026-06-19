namespace BricklePlatform.Domain.DTOs
{
    public class InvestmentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid LeasingId { get; set; }
        public decimal Amount { get; set; }
        public decimal BricksCount { get; set; }
        public string BricksName { get; set; }
        public int PaymentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public LeasingDto? Leasing { get; set; }
    }
}