using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Payment;

public record CreatePaymentCommand
    (
        HeaderRequestModel Header,
        PaymentDto Body
    ) : IRequest<CreatePaymentResponse>;