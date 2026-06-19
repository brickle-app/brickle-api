using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Leasing;

public record UpdateLeasingCommand(Guid Id, UpdateLeasingDto LeasingDto) : IRequest<LeasingDto>;