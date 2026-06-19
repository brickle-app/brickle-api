namespace BricklePlatform.Domain.DTOs;

/// <summary>
/// Pago final (valor residual): la API firma con <c>Web3Settings.WalletPrivateKey</c> para enviar la transacción (gas).
/// On-chain, <c>makeLastLeasingPayment</c> acumula residual + <c>finalPaymentAmount</c> en <c>totalClaimableByUser</c> (reclamo vía <c>claimEarnings</c>, igual que cuotas).
/// </summary>
public class FinalizeResidualPaymentDto
{
    public Guid UserLeasingAgreementId { get; set; }

    /// <summary>
    /// Opcional. Dirección del arrendatario en el contrato; por defecto la wallet del usuario del acuerdo.
    /// </summary>
    public string? ClientAddress { get; set; }
}
