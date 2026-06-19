using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces;

public interface ICampaignRepository
{
  Task<Campaign?> GetByIdAsync(Guid id);
  Task<IEnumerable<Campaign>> GetAllAsync();
  Task<Campaign?> GetByLeasingIdAsync(Guid leasingId);
  Task<Campaign> AddAsync(Campaign campaign);
  Task UpdateAsync(Campaign campaign);
  Task DeleteAsync(Guid id);
}