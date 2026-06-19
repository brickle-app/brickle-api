using System.Numerics;
using BricklePlatform.Domain.DTOs;

namespace BricklePlatform.Domain.Interfaces;

public interface ILeasingCoreService
{
    /// <summary>
    /// Obtiene el baseToken (dirección del ERC20) del contrato LeasingCore en blockchain.
    /// Fuente de verdad para pagos - evita desincronización con Campaign.BaseToken en DB.
    /// </summary>
    Task<string?> GetBaseTokenAsync(string leasingCoreAddress);

    /// <summary>
    /// Obtiene el monto mensual esperado (totalMonthlyPayment) del LeasingCore.
    /// El monto enviado en el pago debe coincidir con este valor para evitar CommitFailed.
    /// </summary>
    Task<BigInteger?> GetExpectedMonthlyPaymentAsync(string leasingCoreAddress);

    /// <summary>
    /// Obtiene el monto esperado del pago actual según el estado del contrato.
    /// - Si hay cuotas mensuales pendientes: devuelve totalMonthlyPayment.
    /// - Si todas las cuotas mensuales están pagadas y falta el residual: devuelve residualValue.
    /// - Si no hay pagos pendientes: devuelve null.
    /// </summary>
    Task<ExpectedPaymentResult?> GetExpectedPaymentAsync(string leasingCoreAddress);

    /// <summary>
    /// Obtiene el balance ERC20 de una wallet vía <c>balanceOf</c>.
    /// Devuelve <c>null</c> si la lectura en cadena falla; no debe interpretarse como saldo cero.
    /// </summary>
    Task<BigInteger?> GetErc20BalanceAsync(string tokenAddress, string walletAddress);

    /// <summary>
    /// Obtiene el mes actual (currentMonth) del LeasingCore - cuántas cuotas mensuales ha recibido el contrato.
    /// Usado para sincronizar el conteo de cuotas pagas en reclamaciones acumuladas.
    /// </summary>
    Task<BigInteger?> GetCurrentMonthAsync(string leasingCoreAddress);

    /// <summary>
    /// Obtiene el estado completo del contrato para el admin: monto esperado, si es pago residual,
    /// currentMonth, termMonths, lastPaymentMade.
    /// </summary>
    Task<LeasingContractStateDto?> GetLeasingContractStateAsync(string leasingCoreAddress);

    /// <summary>
    /// Ejecuta <c>makeLastLeasingPayment(clientAddress, residualValue)</c> en LeasingCore (acumula residual + incentivo final a inversores) firmando con la clave indicada.
    /// </summary>
    Task<string> SendMakeLastLeasingPaymentAsync(string privateKey, string leasingCoreAddress, string clientAddress, BigInteger residualValueWei, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>true</c> si el leasing debe omitirse en listas de activos: cierre final on-chain (<c>lastPaymentMade</c>) y sin <c>getClaimableEarnings</c> pendiente para la wallet del inversor.
    /// Si falla la lectura en cadena, devuelve <c>false</c> (se muestra la inversión).
    /// </summary>
    Task<bool> ShouldOmitFromActiveInvestmentsListAsync(string leasingCoreAddress, string investorWalletAddress, CancellationToken cancellationToken = default);
}
