using BricklePlatform.Domain.Models;

namespace BricklePlatform.Domain.DTOs;

public class PaymentDto
{
    public Guid UserLeasingAgreementId { get; set; }
    public string PaymentAmount { get; set; }
    public int Deadline { get; set; }
    public PermitSignatureDto PermitSignature { get; set; }
    /// <summary>
    /// Dirección de la wallet que firmó el permit (debe coincidir con el owner en EIP-2612).
    /// Si no se envía, se usa la wallet del usuario del acuerdo; PaymentWalletAddress solo como último recurso.
    /// </summary>
    public string? Sender { get; set; }
}