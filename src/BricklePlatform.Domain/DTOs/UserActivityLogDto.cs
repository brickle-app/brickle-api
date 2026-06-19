namespace BricklePlatform.Domain.DTOs;

public class UserActivityLogDto
{
    public Guid UserId { get; set; }
    /// <summary>
    /// Tipo de movimiento. Para rentas reclamadas usar:
    /// INVESTMENT-RETURN-INTEREST (solo intereses) o INVESTMENT-RETURN-CAPITAL (solo capital amortizado).
    /// El tipo legacy INVESTMENT-RETURN se trata como interés puro por backward-compat.
    /// </summary>
    public string Type { get; set; } = string.Empty; // INVESTMENT | INVESTMENT-RETURN | INVESTMENT-RETURN-INTEREST | INVESTMENT-RETURN-CAPITAL | RECHARGE | WITHDRAW
    public decimal TxAmount { get; set; }
    public string Status { get; set; } = string.Empty; // SUCCESS | ERROR
    public string Receipt { get; set; } = string.Empty; // Image URL
    public string Hash { get; set; } = string.Empty; // Blockchain transaction hash
    public string Reference { get; set; } = string.Empty; // Description of operation
    public Guid? LeasingId { get; set; } // Leasing ID for filtering
    public DateTime Timestamp { get; set; }
}