using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces
{
    public interface IInvestmentRepository
    {
        Task<Investment> CreateAsync(Investment investment);
        Task<Investment?> GetByIdAsync(Guid id);
        Task<Investment?> GetByUserIdAndLeasingIdAsync(Guid userId, Guid leasingId);
        Task<IEnumerable<InvestmentDto>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<InvestmentDto>> GetByLeasingIdAsync(Guid leasingId);
        Task<IEnumerable<User>> GetInvestorsByLeasingIdAsync(Guid leasingId);
        Task<IEnumerable<InvestmentDto>> GetAllAsync();
        Task<Investment> UpdateAsync(Investment investment);
        Task<bool> DeleteAsync(Guid id);
    }
}