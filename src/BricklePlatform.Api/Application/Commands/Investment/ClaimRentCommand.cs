using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Investment;

public record ClaimRentCommand(
    HeaderRequestModel Header,
    Guid UserId,
    Guid LeasingId,
    ClaimRentDto ClaimRentDto
) : IRequest<bool>;