using BricklePlatform.Api.Application.Dtos;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.UserLeasingAgreement;

public record UpdateUserLeasingAgreementCommand(Guid Id, UpdateUserLeasingAgreementDto AgreementDto) : IRequest<UserLeasingAgreementDto>; 