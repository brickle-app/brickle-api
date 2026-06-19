using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Leasing;

public record CreateLeasingCommand(CreateLeasingDto LeasingDto) : IRequest<LeasingDto>;