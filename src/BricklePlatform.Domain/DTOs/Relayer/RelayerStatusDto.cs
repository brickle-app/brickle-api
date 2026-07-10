namespace BricklePlatform.Domain.DTOs.Relayer;

public class RelayerStatusDto
{
    public bool Configured { get; set; }
    public string Network { get; set; } = string.Empty;
    public bool RpcConfigured { get; set; }
    public string PaymasterAddress { get; set; } = string.Empty;
    public string RelayerAddress { get; set; } = string.Empty;
    public string NativeBalance { get; set; } = string.Empty;
    public bool HasMinimumBalance { get; set; }
    public string? ErrorMessage { get; set; }
}
