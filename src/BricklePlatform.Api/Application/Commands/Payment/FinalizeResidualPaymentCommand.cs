using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Payment;

public record FinalizeResidualPaymentCommand(HeaderRequestModel Header, FinalizeResidualPaymentDto Body)
    : IRequest<CreatePaymentResponse>;
