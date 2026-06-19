using BricklePlatform.Api.Models;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Company;

public record DeleteCompanyCommand
    (
        HeaderRequestModel Header,
        Guid Id
    ) : IRequest<Unit>;