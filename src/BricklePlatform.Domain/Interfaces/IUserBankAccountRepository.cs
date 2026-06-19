using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces;

public interface IUserBankAccountRepository
{
    Task<UserBankAccount> CreateAsync(UserBankAccount userBankAccount);
    Task<UserBankAccountDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<UserBankAccount>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserBankAccountSummaryDto>> GetSummaryByUserIdAsync(Guid userId);
    Task<UserBankAccount?> GetByUserIdAndAccountNumberAsync(Guid userId, string accountNumber);
    Task<UserBankAccount> UpdateAsync(UserBankAccount userBankAccount);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsAsync(Guid userId, string accountNumber);
    Task<int> GetAccountCountByUserIdAsync(Guid userId);
}