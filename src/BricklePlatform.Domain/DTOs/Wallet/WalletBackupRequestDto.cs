namespace BricklePlatform.Domain.DTOs.Wallet;

public class WalletBackupRequestDto
{
    public string WalletAddress { get; set; } = string.Empty;
    public string EncryptedPrivateKey { get; set; } = string.Empty;
    public string EncryptionVersion { get; set; } = string.Empty;
    public string Cipher { get; set; } = string.Empty;
    public string Kdf { get; set; } = string.Empty;
    public Dictionary<string, object> KdfParams { get; set; } = new();
}
