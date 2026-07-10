namespace BricklePlatform.Domain.DTOs.Relayer;

public class RelayerTransactionResponseDto
{
    public string Hash { get; set; } = string.Empty;
    public bool Status { get; set; }
    public string? ErrorMessage { get; set; }
    public long? BlockNumber { get; set; }
    public string? GasUsed { get; set; }
    public string? EffectiveGasPrice { get; set; }
}
