using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Infrastructure.Repositories;

public class UserBankAccountRepository : IUserBankAccountRepository
{
    private readonly ApplicationDbContext _context;

    public UserBankAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserBankAccount> CreateAsync(UserBankAccount userBankAccount)
    {
        _context.UserBankAccounts.Add(userBankAccount);
        await _context.SaveChangesAsync();
        return userBankAccount;
    }

    public async Task<UserBankAccountDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.UserBankAccounts
            .Include(uba => uba.User)
            .FirstOrDefaultAsync(uba => uba.Id == id);

        if (entity == null)
            return null;

        return new UserBankAccountDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            BankName = entity.BankName,
            AccountType = entity.AccountType,
            AccountNumber = entity.AccountNumber,
            AccountHolder = entity.AccountHolder,
            AccountDocument = entity.AccountDocument,
            AccountImage = entity.AccountImage,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public async Task<IEnumerable<UserBankAccount>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserBankAccounts
            .Where(uba => uba.UserId == userId)
            .OrderByDescending(uba => uba.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserBankAccountSummaryDto>> GetSummaryByUserIdAsync(Guid userId)
    {
        return await _context.UserBankAccounts
            .Where(uba => uba.UserId == userId)
            .OrderByDescending(uba => uba.CreatedAt)
            .Select(uba => new UserBankAccountSummaryDto
            {
                Id = uba.Id,
                BankName = uba.BankName,
                AccountType = uba.AccountType,
                MaskedAccountNumber = MaskAccountNumber(uba.AccountNumber),
                AccountHolder = uba.AccountHolder,
                CreatedAt = uba.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<UserBankAccount?> GetByUserIdAndAccountNumberAsync(Guid userId, string accountNumber)
    {
        return await _context.UserBankAccounts
            .FirstOrDefaultAsync(uba => uba.UserId == userId && uba.AccountNumber == accountNumber);
    }

    public async Task<UserBankAccount> UpdateAsync(UserBankAccount userBankAccount)
    {
        _context.UserBankAccounts.Update(userBankAccount);
        await _context.SaveChangesAsync();
        return userBankAccount;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userBankAccount = await _context.UserBankAccounts.FindAsync(id);
        if (userBankAccount == null)
            return false;

        _context.UserBankAccounts.Remove(userBankAccount);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.UserBankAccounts
            .AnyAsync(uba => uba.Id == id);
    }

    public async Task<bool> ExistsAsync(Guid userId, string accountNumber)
    {
        return await _context.UserBankAccounts
            .AnyAsync(uba => uba.UserId == userId && uba.AccountNumber == accountNumber);
    }

    public async Task<int> GetAccountCountByUserIdAsync(Guid userId)
    {
        return await _context.UserBankAccounts
            .CountAsync(uba => uba.UserId == userId);
    }

    private static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length <= 4)
            return accountNumber;

        var visibleDigits = accountNumber[^4..]; // Last 4 digits
        var maskedLength = accountNumber.Length - 4;
        return new string('*', maskedLength) + visibleDigits;
    }
}