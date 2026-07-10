using BricklePlatform.Domain.Models;

namespace BricklePlatform.Domain.DTOs.Relayer;

public class RelayerSponsorRequestDto
{
    public string Command { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Campaign { get; set; } = string.Empty;
    public string LeasingCore { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string Fee { get; set; } = string.Empty;
    public string Deadline { get; set; } = string.Empty;
    public PermitSignatureDto PermitSignature { get; set; } = new();
}

public class DefenderStyleRelayerRequestDto
{
    public List<RelayerSponsorRequestDto> Params { get; set; } = new();
}
