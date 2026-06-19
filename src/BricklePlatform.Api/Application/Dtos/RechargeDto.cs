using System.ComponentModel.DataAnnotations;

namespace BricklePlatform.Api.Application.Dtos;

public class CreateRechargeDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Amount { get; set; }

    [Required]
    [Url(ErrorMessage = "La URL del comprobante no es válida")]
    public string Receipt { get; set; } = string.Empty;

    public string? Reference { get; set; } = "Recarga";
}

public class UpdateRechargeDto
{
    [Required]
    public string Status { get; set; } = string.Empty;

    public string? Hash { get; set; }

    public string? Notes { get; set; }
}

public class RechargeResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = "RECHARGE";
    public decimal TxAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Receipt { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}