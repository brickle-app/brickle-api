using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Infrastructure.Repositories;

public class WalletBackupRepository : IWalletBackupRepository
{
    private readonly ApplicationDbContext _context;

    public WalletBackupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WalletBackup?> GetByUserIdAsync(Guid userId)
    {
        return await _context.WalletBackups.FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task<WalletBackup> UpsertAsync(WalletBackup walletBackup)
    {
        var existing = await GetByUserIdAsync(walletBackup.UserId);
        if (existing == null)
        {
            _context.WalletBackups.Add(walletBackup);
            await _context.SaveChangesAsync();
            return walletBackup;
        }

        existing.Update(
            walletBackup.WalletAddress,
            walletBackup.EncryptedPrivateKey,
            walletBackup.EncryptionVersion,
            walletBackup.Cipher,
            walletBackup.Kdf,
            walletBackup.KdfParamsJson);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task UpdateAsync(WalletBackup walletBackup)
    {
        _context.WalletBackups.Update(walletBackup);
        await _context.SaveChangesAsync();
    }
}
