using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Infrastructure.Repositories;

public class UserDocumentRepository : IUserDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public UserDocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDocument?> GetByIdAsync(Guid id)
    {
        return await _context.UserDocuments
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<UserDocument>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserDocuments
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserDocument>> GetAllAsync(string? status = null)
    {
        var query = _context.UserDocuments
            .Include(d => d.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(d => d.Status == status);
        }

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserDocument> AddAsync(UserDocument document)
    {
        await _context.UserDocuments.AddAsync(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<UserDocument> UpdateAsync(UserDocument document)
    {
        _context.UserDocuments.Update(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var document = await _context.UserDocuments.FindAsync(id);
        if (document == null) return false;

        _context.UserDocuments.Remove(document);
        await _context.SaveChangesAsync();
        return true;
    }
}
