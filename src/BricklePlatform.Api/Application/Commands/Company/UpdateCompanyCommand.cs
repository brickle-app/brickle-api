using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Company;

public record UpdateCompanyCommand
    (
        HeaderRequestModel Header,
        Guid Id,
        UpdateCompanyDto Body
    ) : IRequest<CompanyDto>;