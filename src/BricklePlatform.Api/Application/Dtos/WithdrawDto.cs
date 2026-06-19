using System.ComponentModel.DataAnnotations;

namespace BricklePlatform.Api.Application.Dtos;

public class CreateWithdrawDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Amount { get; set; }

    public string? Reference { get; set; } = "Retiro";
}

public class UpdateWithdrawDto
{
    [Required]
    public string Status { get; set; } = string.Empty;

    public string? Hash { get; set; }

    public string? Notes { get; set; }
}

public class WithdrawResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = "WITHDRAW";
    public decimal TxAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Receipt { get; set; } = "N/A";
    public string Hash { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}