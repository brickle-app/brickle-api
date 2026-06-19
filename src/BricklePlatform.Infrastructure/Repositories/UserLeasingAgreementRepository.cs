using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BricklePlatform.Domain.Common;

namespace BricklePlatform.Infrastructure.Repositories;

public class UserLeasingAgreementRepository : IUserLeasingAgreementRepository
{
    private readonly ApplicationDbContext _context;

    public UserLeasingAgreementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserLeasingAgreement?> GetByIdAsync(Guid id)
    {
        return await _context.UserLeasingAgreements
            .Include(a => a.User)
            .Include(a => a.Leasing)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<UserLeasingAgreement>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserLeasingAgreements
            .Include(a => a.User)
            .Include(a => a.Leasing)
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    public async Task<UserLeasingAgreement?> GetByLeasingIdAsync(Guid leasingId)
    {
        return await _context.UserLeasingAgreements
            .Include(a => a.User)
                .ThenInclude(u => u.Company)
            .Include(a => a.Leasing)
            .Where(a => a.LeasingId == leasingId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserLeasingAgreement>> GetAllByLeasingIdAsync(Guid leasingId)
    {
        return await _context.UserLeasingAgreements
            .Include(a => a.User)
                .ThenInclude(u => u.Company)
            .Include(a => a.Leasing)
            .Where(a => a.LeasingId == leasingId)
            .ToListAsync();
    }

    public async Task<UserLeasingAgreement> AddAsync(UserLeasingAgreement agreement)
    {
        await _context.UserLeasingAgreements.AddAsync(agreement);
        await _context.SaveChangesAsync();
        return agreement;
    }

    public async Task<UserLeasingAgreement> UpdateAsync(UserLeasingAgreement agreement)
    {
        _context.UserLeasingAgreements.Update(agreement);
        await _context.SaveChangesAsync();
        return agreement;
    }

    public async Task<bool> UpdateRemainingBalanceAsync(Guid id, decimal remainingBalance)
    {
        var rowsAffected = await _context.UserLeasingAgreements
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.RemainingBalance, remainingBalance)
                .SetProperty(a => a.UpdatedAt, DateTime.UtcNow));

        return rowsAffected > 0;
    }

    public async Task<bool> ProcessPaymentAsync(Guid id, decimal paymentAmount)
    {
        var agreement = await _context.UserLeasingAgreements
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agreement == null)
            return false;

        agreement.ProcessPayment(paymentAmount);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var agreement = await GetByIdAsync(id);
        if (agreement != null)
        {
            _context.UserLeasingAgreements.Remove(agreement);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
}