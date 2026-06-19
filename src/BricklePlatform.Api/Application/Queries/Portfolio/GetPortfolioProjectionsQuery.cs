using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Portfolio
{
    public record GetPortfolioProjectionsQuery(
        Guid UserId,
        decimal CurrentValue,
        int ProjectionMonths,
        decimal ExpectedAnnualReturn,
        DateOnly StartDate,
        decimal InvestedPrincipal
    ) : IRequest<List<ProjectionPointDto>>;
}