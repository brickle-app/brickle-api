using BricklePlatform.Api.Models;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.User;

public record DeleteUserCommand
    (
        HeaderRequestModel Header,
        Guid UserId
    ) : IRequest<Unit>;