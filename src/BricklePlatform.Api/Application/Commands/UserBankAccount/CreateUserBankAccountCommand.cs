using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.UserBankAccount;

public record CreateUserBankAccountCommand(
    HeaderRequestModel Header,
    CreateUserBankAccountDto CreateUserBankAccountDto
) : IRequest<UserBankAccountDto>;