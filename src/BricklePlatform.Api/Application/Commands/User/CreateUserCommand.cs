using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.User;

public record CreateUserCommand
    (
        HeaderRequestModel Header,
        CreateUserDto Body
    ) : IRequest<UserDto>;