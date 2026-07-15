using System.Text.Json;
using System.Text.RegularExpressions;
using BricklePlatform.Domain.DTOs.Wallet;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;

namespace BricklePlatform.Infrastructure.Services;

public class WalletBackupService : IWalletBackupService
{
    private const string PasswordBackupEncryptionVersion = "ethers-keystore-v1";
    private const string BackupCodeEncryptionVersion = "ethers-keystore-v1-backup-code";
    private static readonly Regex PlainPrivateKeyPattern = new("^0x[a-fA-F0-9]{64}$", RegexOptions.Compiled);
    private readonly IUserRepository _userRepository;
    private readonly IWalletBackupRepository _walletBackupRepository;

    public WalletBackupService(IUserRepository userRepository, IWalletBackupRepository walletBackupRepository)
    {
        _userRepository = userRepository;
        _walletBackupRepository = walletBackupRepository;
    }

    public async Task<WalletBackupResponseDto> SaveAsync(Guid userId, WalletBackupRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("Authenticated user was not found");

        ValidateRequestForUser(user, request);

        var backup = WalletBackup.Create(
            userId,
            request.WalletAddress.Trim(),
            request.EncryptedPrivateKey,
            request.EncryptionVersion.Trim(),
            request.Cipher.Trim(),
            request.Kdf.Trim(),
            JsonSerializer.Serialize(request.KdfParams));

        var saved = await _walletBackupRepository.UpsertAsync(backup);
        return ToDto(saved);
    }

    public async Task<WalletBackupResponseDto> UpgradeActiveWalletAsync(Guid userId, WalletBackupRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("Authenticated user was not found");

        ValidateEncryptedBackupRequest(request, requireBackupCodeVersion: true);

        var walletAddress = request.WalletAddress.Trim();
        var owner = await _userRepository.GetByWalletAddressAsync(walletAddress);
        if (owner != null && owner.Id != userId)
            throw new InvalidOperationException("Wallet address already belongs to another user");

        var backup = WalletBackup.Create(
            userId,
            walletAddress,
            request.EncryptedPrivateKey,
            request.EncryptionVersion.Trim(),
            request.Cipher.Trim(),
            request.Kdf.Trim(),
            JsonSerializer.Serialize(request.KdfParams));

        var saved = await _walletBackupRepository.UpsertAsync(backup);

        user.Update(walletAddress: walletAddress);
        await _userRepository.UpdateAsync(user);

        return ToDto(saved);
    }

    public async Task<WalletBackupResponseDto> GetAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("Authenticated user was not found");

        var backup = await _walletBackupRepository.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Wallet backup not found");

        if (string.IsNullOrWhiteSpace(user.WalletAddress) ||
            !string.Equals(user.WalletAddress.Trim(), backup.WalletAddress.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Wallet backup does not belong to authenticated user");
        }

        backup.MarkRestored();
        await _walletBackupRepository.UpdateAsync(backup);

        return ToDto(backup);
    }

    private static void ValidateRequestForUser(User user, WalletBackupRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(user.WalletAddress))
            throw new InvalidOperationException("Authenticated user does not have a wallet address");

        if (!string.Equals(user.WalletAddress.Trim(), request.WalletAddress?.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Wallet address does not belong to authenticated user");

        ValidateEncryptedBackupRequest(request, requireBackupCodeVersion: false);
    }

    private static void ValidateEncryptedBackupRequest(WalletBackupRequestDto request, bool requireBackupCodeVersion)
    {
        if (string.IsNullOrWhiteSpace(request.EncryptedPrivateKey))
            throw new InvalidOperationException("Encrypted wallet backup is required");

        if (PlainPrivateKeyPattern.IsMatch(request.EncryptedPrivateKey.Trim()))
            throw new InvalidOperationException("Plaintext private keys are not accepted");

        if (requireBackupCodeVersion && request.EncryptionVersion != BackupCodeEncryptionVersion)
            throw new InvalidOperationException("Wallet upgrades require backup-code encryption version");

        if (!requireBackupCodeVersion &&
            request.EncryptionVersion != PasswordBackupEncryptionVersion &&
            request.EncryptionVersion != BackupCodeEncryptionVersion)
            throw new InvalidOperationException("Unsupported wallet backup encryption version");

        if (request.Cipher != "ethers-json-keystore")
            throw new InvalidOperationException("Unsupported wallet backup cipher");

        if (request.Kdf != "scrypt")
            throw new InvalidOperationException("Unsupported wallet backup KDF");
    }

    private static WalletBackupResponseDto ToDto(WalletBackup backup)
    {
        return new WalletBackupResponseDto
        {
            WalletAddress = backup.WalletAddress,
            EncryptedPrivateKey = backup.EncryptedPrivateKey,
            EncryptionVersion = backup.EncryptionVersion,
            Cipher = backup.Cipher,
            Kdf = backup.Kdf,
            KdfParams = JsonSerializer.Deserialize<Dictionary<string, object>>(backup.KdfParamsJson) ?? new Dictionary<string, object>()
        };
    }
}
