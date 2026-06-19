using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Company;

public record GetAllCompaniesQuery
    (
        HeaderRequestModel Header
    ) : IRequest<IEnumerable<CompanyDto>>;