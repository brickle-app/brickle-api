using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id);
    Task<Company?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<Company> AddAsync(Company company);
    Task UpdateAsync(Company company);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsByUserIdAsync(Guid userId);
    Task<IEnumerable<Company>> GetAllByUserIdAsync(Guid userId);
}