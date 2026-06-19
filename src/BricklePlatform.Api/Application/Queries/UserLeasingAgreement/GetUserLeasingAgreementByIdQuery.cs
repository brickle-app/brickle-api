using BricklePlatform.Api.Application.Dtos;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.UserLeasingAgreement;

public record GetUserLeasingAgreementByIdQuery(Guid Id) : IRequest<UserLeasingAgreementDto>; 