using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Investment
{
    public class GetInvestmentByIdQuery : IRequest<InvestmentDto?>
    {
        public Guid Id { get; set; }

        public GetInvestmentByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}