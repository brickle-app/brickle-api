using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Company;

public record GetCompaniesByUserIdQuery
    (
        HeaderRequestModel Header,
        Guid UserId
    ) : IRequest<IEnumerable<CompanyDto>>;
