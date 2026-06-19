using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Investment
{
    public class GetInvestmentsByUserIdQuery : IRequest<IEnumerable<InvestmentDto>>
    {
        public Guid UserId { get; set; }

        public GetInvestmentsByUserIdQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}