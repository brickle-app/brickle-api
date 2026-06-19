using BricklePlatform.Api.Application.Dtos;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Investment
{
    public class CreateInvestmentCommand : IRequest<CreateInvestmentDto>
    {
        public Guid UserId { get; set; }
        public Guid LeasingId { get; set; }
        public decimal Amount { get; set; }
        public decimal BricksCount { get; set; }
        public string BricksName { get; set; } = null!;
    }
}