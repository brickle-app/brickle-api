using BricklePlatform.Api.Application.Dtos;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.UserLeasingAgreement;

public record GetUserLeasingAgreementsByUserIdQuery(Guid UserId) : IRequest<IEnumerable<UserLeasingAgreementDto>>;