using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetByWalletAddressAsync(string walletAddress);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> SearchUsersAsync(string? email = null, string? phoneNumber = null, Guid? excludeUserId = null);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<List<User>> GetUsersWithTokensAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}