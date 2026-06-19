using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.UserBankAccount;

public record UpdateUserBankAccountCommand(
    HeaderRequestModel Header,
    Guid Id,
    UpdateUserBankAccountDto UpdateUserBankAccountDto
) : IRequest<UserBankAccountDto>;