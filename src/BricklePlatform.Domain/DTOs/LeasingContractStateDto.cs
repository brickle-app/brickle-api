using System.Numerics;

namespace BricklePlatform.Domain.DTOs;

/// <summary>
/// Estado actual del contrato LeasingCore para mostrar en admin.
/// Incluye dirección y totalSupply del LeasingToken (participación), distinto del token base en el Core.
/// </summary>
public record LeasingContractStateDto(
    BigInteger ExpectedAmount,
    bool IsResidualPayment,
    int CurrentMonth,
    int TermMonths,
    bool LastPaymentMade,
    BigInteger? ResidualValue = null,
    BigInteger? FinalPaymentAmount = null,
    string? LeasingTokenAddress = null,
    BigInteger? LeasingTokenTotalSupply = null);
