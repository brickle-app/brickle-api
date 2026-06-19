using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.UserBankAccount;

public record GetUserBankAccountsQuery(
    HeaderRequestModel Header,
    Guid UserId
) : IRequest<IEnumerable<UserBankAccountSummaryDto>>;