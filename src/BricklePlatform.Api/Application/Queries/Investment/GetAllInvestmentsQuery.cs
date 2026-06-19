using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Investment
{
    public record GetAllInvestmentsQuery() : IRequest<IEnumerable<InvestmentDto>>;
}