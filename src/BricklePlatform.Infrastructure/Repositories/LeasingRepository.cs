using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Enums;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Infrastructure.Repositories;

public class LeasingRepository : ILeasingRepository
{
    private readonly ApplicationDbContext _context;

    public LeasingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Leasing?> GetByIdAsync(Guid id)
    {
        return await _context.Leasings
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Leasing?> GetByIdWithAgreementAsync(Guid id)
    {
        return await _context.Leasings
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Leasing>> GetAllAsync()
    {
        return await _context.Leasings
            .ToListAsync();
    }

    public async Task<IEnumerable<Leasing>> GetAllWithAgreementsAsync()
    {
        return await _context.Leasings
            .ToListAsync();
    }

    public IQueryable<Leasing> GetQueryable()
    {
        return _context.Leasings.AsQueryable();
    }

    public async Task<Leasing> CreateAsync(Leasing leasing)
    {
        await _context.Leasings.AddAsync(leasing);
        await _context.SaveChangesAsync();
        return leasing;
    }

    public async Task<Leasing> UpdateAsync(Leasing leasing)
    {
        _context.Leasings.Update(leasing);
        await _context.SaveChangesAsync();
        return leasing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        Leasing? leasing = await GetByIdAsync(id);
        if (leasing != null)
        {
            _context.Leasings.Remove(leasing);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<(IEnumerable<Leasing> Items, int TotalCount)> GetFilteredAsync(
        IEnumerable<LeasingTypeEnum>? categories,
        int page,
        int limit,
        bool? active = null)
    {
        var query = _context.Leasings.AsQueryable();

        if (categories != null && categories.Any())
        {
            query = query.Where(l => categories.Contains(l.Type));
        }

        if (active.HasValue)
        {
            query = query.Where(l => l.Active == active.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return (items.AsEnumerable(), totalCount);
    }

    public async Task<IEnumerable<Leasing>> GetAllActiveAsync(bool Active)
    {
        return await _context.Leasings
            .Where(l => l.Active == Active)
            .ToListAsync();
    }
}