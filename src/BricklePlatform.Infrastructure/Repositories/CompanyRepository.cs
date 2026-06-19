using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Company?> GetByIdAsync(Guid id)
    {
        return await _context.Companies
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Company?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Companies
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await _context.Companies
            .Include(c => c.User)
            .ToListAsync();
    }

    public async Task<Company> AddAsync(Company company)
    {
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return company;
    }

    public async Task UpdateAsync(Company company)
    {
        _context.Companies.Update(company);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company != null)
        {
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId)
    {
        return await _context.Companies
            .AnyAsync(c => c.UserId == userId);
    }

    public async Task<IEnumerable<Company>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.Companies
            .Include(c => c.User)
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }
}