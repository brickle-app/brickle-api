namespace BricklePlatform.Domain.Entities;

public class LogEntry
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string LeasingId { get; set; }
    public string Hash { get; set; }
}