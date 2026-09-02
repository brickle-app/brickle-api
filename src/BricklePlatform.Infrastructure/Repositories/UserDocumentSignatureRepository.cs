using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Infrastructure.Repositories;

public class UserDocumentSignatureRepository : IUserDocumentSignatureRepository
{
    private readonly ApplicationDbContext _context;

    public UserDocumentSignatureRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDocumentSignature?> GetByUserAndDocumentTypeAsync(Guid userId, string documentType)
    {
        return await _context.UserDocumentSignatures
            .FirstOrDefaultAsync(s => s.UserId == userId && s.DocumentType == documentType);
    }

    public async Task<IEnumerable<UserDocumentSignature>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserDocumentSignatures
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SignedAt)
            .ToListAsync();
    }

    public async Task<UserDocumentSignature> AddAsync(UserDocumentSignature signature)
    {
        await _context.UserDocumentSignatures.AddAsync(signature);
        await _context.SaveChangesAsync();
        return signature;
    }

    public async Task<UserDocumentSignature> UpdateAsync(UserDocumentSignature signature)
    {
        _context.UserDocumentSignatures.Update(signature);
        await _context.SaveChangesAsync();
        return signature;
    }
}
