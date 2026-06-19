using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Infrastructure.Repositories;

public class UserContactRepository : IUserContactRepository
{
    private readonly ApplicationDbContext _context;

    public UserContactRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserContact>> GetContactsByUserIdAsync(Guid userId)
    {
        return await _context.UserContacts
            .Include(uc => uc.Contact)
            .Where(uc => uc.UserId == userId)
            .ToListAsync();
    }

    public async Task<UserContact?> GetContactAsync(Guid userId, Guid contactId)
    {
        return await _context.UserContacts
            .Include(uc => uc.Contact)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ContactId == contactId);
    }

    public async Task<UserContact> AddContactAsync(UserContact userContact)
    {
        await _context.UserContacts.AddAsync(userContact);
        await _context.SaveChangesAsync();
        return userContact;
    }

    public async Task DeleteContactAsync(Guid userId, Guid contactId)
    {
        var userContact = await GetContactAsync(userId, contactId);
        if (userContact != null)
        {
            _context.UserContacts.Remove(userContact);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ContactExistsAsync(Guid userId, Guid contactId)
    {
        return await _context.UserContacts
            .AnyAsync(uc => uc.UserId == userId && uc.ContactId == contactId);
    }
} 