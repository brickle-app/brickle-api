using System.ComponentModel.DataAnnotations;

namespace BricklePlatform.Api.Application.Models;

public class NotifyIncomingTransferRequest
{
    [Required(ErrorMessage = "La dirección del destinatario es requerida.")]
    public required string RecipientWalletAddress { get; set; }

    [Required(ErrorMessage = "El monto es requerido.")]
    public required string Amount { get; set; }

    public string? TransactionHash { get; set; }
}
