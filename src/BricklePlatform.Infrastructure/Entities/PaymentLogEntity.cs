using Azure;
using Azure.Data.Tables;

namespace BricklePlatform.Infrastructure.Entities;

public class PaymentLogEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public string UserLeasingAgreementId { get; set; }
    public string Hash { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal RemainingBalance { get; set; }
    public string LeasingContractAddress { get; set; }
    public string UserWallet { get; set; }
    public bool Status { get; set; }
}