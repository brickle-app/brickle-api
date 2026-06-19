using BricklePlatform.Infrastructure.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Infrastructure.Repositories;

public class ApiKeyRepository : IApiKeyRepository
{
    private readonly ApplicationDbContext _context;

    public ApiKeyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<string>> GetActiveApiKeysAsync()
    {
        return await _context.ApiKeys
            .Where(k => k.IsActive && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow))
            .Select(k => k.Key)
            .ToListAsync();
    }

    public async Task<bool> ValidateApiKeyAsync(string apiKey)
    {
        return await _context.ApiKeys
            .AnyAsync(k => k.Key == apiKey &&
                          k.IsActive &&
                          (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow));
    }
}