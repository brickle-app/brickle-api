using System.Numerics;

namespace BricklePlatform.Domain.DTOs;

/// <summary>
/// Resultado del monto esperado para el pago actual del leasing.
/// </summary>
public record ExpectedPaymentResult(BigInteger Amount, bool IsResidualPayment);
