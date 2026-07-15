namespace BricklePlatform.Domain.Entities;

public class WalletBackup
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string WalletAddress { get; private set; } = string.Empty;
    public string EncryptedPrivateKey { get; private set; } = string.Empty;
    public string EncryptionVersion { get; private set; } = string.Empty;
    public string Cipher { get; private set; } = string.Empty;
    public string Kdf { get; private set; } = string.Empty;
    public string KdfParamsJson { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastRestoredAt { get; private set; }

    public User User { get; private set; } = null!;

    private WalletBackup()
    {
    }

    public static WalletBackup Create(
        Guid userId,
        string walletAddress,
        string encryptedPrivateKey,
        string encryptionVersion,
        string cipher,
        string kdf,
        string kdfParamsJson)
    {
        return new WalletBackup
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WalletAddress = walletAddress,
            EncryptedPrivateKey = encryptedPrivateKey,
            EncryptionVersion = encryptionVersion,
            Cipher = cipher,
            Kdf = kdf,
            KdfParamsJson = kdfParamsJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string walletAddress,
        string encryptedPrivateKey,
        string encryptionVersion,
        string cipher,
        string kdf,
        string kdfParamsJson)
    {
        WalletAddress = walletAddress;
        EncryptedPrivateKey = encryptedPrivateKey;
        EncryptionVersion = encryptionVersion;
        Cipher = cipher;
        Kdf = kdf;
        KdfParamsJson = kdfParamsJson;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkRestored()
    {
        LastRestoredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
