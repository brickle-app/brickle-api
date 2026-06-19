using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Company;

public record GetCompanyByIdQuery
    (
        HeaderRequestModel Header,
        Guid Id
    ) : IRequest<CompanyDto?>;