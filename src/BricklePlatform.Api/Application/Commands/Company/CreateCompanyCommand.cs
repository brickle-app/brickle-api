using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Company;

public record CreateCompanyCommand
    (
        HeaderRequestModel Header,
        CreateCompanyDto Body
    ) : IRequest<CompanyDto>;