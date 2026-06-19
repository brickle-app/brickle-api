using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Infrastructure.Repositories;

public class CampaignRepository : ICampaignRepository
{
  private readonly ApplicationDbContext _context;

  public CampaignRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<Campaign?> GetByIdAsync(Guid id)
  {
    return await _context.Campaigns.FindAsync(id);
  }

  public async Task<IEnumerable<Campaign>> GetAllAsync()
  {
    return await _context.Campaigns.ToListAsync();
  }

  public async Task<Campaign?> GetByLeasingIdAsync(Guid leasingId)
  {
    return await _context.Campaigns
            .Where(a => a.LeasingId == leasingId)
            .FirstOrDefaultAsync();
  }

  public async Task<Campaign> AddAsync(Campaign campaign)
  {
    _context.Campaigns.Add(campaign);
    await _context.SaveChangesAsync();
    return campaign;
  }

  public async Task UpdateAsync(Campaign campaign)
  {
    _context.Campaigns.Update(campaign);
    await _context.SaveChangesAsync();
  }

  public async Task DeleteAsync(Guid id)
  {
    var campaign = await GetByIdAsync(id);
    if (campaign != null)
    {
      _context.Campaigns.Remove(campaign);
      await _context.SaveChangesAsync();
    }
  }
}