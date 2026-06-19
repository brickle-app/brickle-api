using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.UserBankAccount;

public record GetUserBankAccountByIdQuery(
    HeaderRequestModel Header,
    Guid Id
) : IRequest<UserBankAccountDto?>;