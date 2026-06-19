namespace BricklePlatform.Api.Application.Dtos
{
    public class CreateInvestmentDto
    {
        public Guid UserId { get; set; }
        public Guid LeasingId { get; set; }
        public decimal Amount { get; set; }
        public decimal BricksCount { get; set; }
        public required string BricksName { get; set; }
    }
}