namespace BricklePlatform.Api.Application.Commands.Payment;

/// <summary>
/// Response after creating a payment: status, tx hash, and updated balance info.
/// ErrorMessage se incluye cuando status es false para facilitar diagnóstico.
/// </summary>
public record CreatePaymentResponse(
    bool Status,
    string Hash,
    decimal PaymentAmount,
    decimal RemainingBalance,
    string? ErrorMessage = null
);
