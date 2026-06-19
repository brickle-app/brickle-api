namespace BricklePlatform.Domain.Models;

public class WalletResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string BlockchainType { get; set; }
    public string NetworkType { get; set; }
    public bool IsDefault { get; set; }
    public string Address { get; set; }
}