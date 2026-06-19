using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Portfolio
{
    public record GetPortfolioOverviewQuery(
        Guid UserId,
        DateOnly From,
        DateOnly To
    ) : IRequest<PortfolioOverviewDto>;
}