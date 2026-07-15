using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces;

public interface IWalletBackupRepository
{
    Task<WalletBackup?> GetByUserIdAsync(Guid userId);
    Task<WalletBackup> UpsertAsync(WalletBackup walletBackup);
    Task UpdateAsync(WalletBackup walletBackup);
}
