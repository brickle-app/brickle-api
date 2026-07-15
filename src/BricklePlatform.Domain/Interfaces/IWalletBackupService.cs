using BricklePlatform.Domain.DTOs.Wallet;

namespace BricklePlatform.Domain.Interfaces;

public interface IWalletBackupService
{
    Task<WalletBackupResponseDto> SaveAsync(Guid userId, WalletBackupRequestDto request);
    Task<WalletBackupResponseDto> UpgradeActiveWalletAsync(Guid userId, WalletBackupRequestDto request);
    Task<WalletBackupResponseDto> GetAsync(Guid userId);
}
