using Azure;
using Azure.Data.Tables;

namespace BricklePlatform.Infrastructure.Entities;

public class UserActivityLogEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // INVESTMENT | INVESTMENT-RETURN | RECHARGE | WITHDRAW
    public double TxAmount { get; set; }
    public string Status { get; set; } = string.Empty; // SUCCESS | ERROR | PENDING
    public string Receipt { get; set; } = string.Empty; // Image URL
    public string Hash { get; set; } = string.Empty; // Blockchain transaction hash
    public string Reference { get; set; } = string.Empty; // Description of operation
    public string LeasingId { get; set; } = string.Empty; // Leasing ID for filtering
    /// <summary>Fecha del movimiento (UTC). Permite histórico correcto en portfolio y seeds QA.</summary>
    public DateTime? EventTime { get; set; }
}