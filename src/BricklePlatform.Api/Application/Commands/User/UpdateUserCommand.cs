using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.User;

public record UpdateUserCommand
    (
        HeaderRequestModel Header,
        Guid UserId,
        UpdateUserDto Body
    ) : IRequest<UserDto>;