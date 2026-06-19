using BricklePlatform.Api.Models;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.UserBankAccount;

public record DeleteUserBankAccountCommand(
    HeaderRequestModel Header,
    Guid Id
) : IRequest<bool>;