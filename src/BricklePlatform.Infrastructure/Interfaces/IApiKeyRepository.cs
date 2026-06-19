using System.Collections.Generic;
using System.Threading.Tasks;

namespace BricklePlatform.Infrastructure.Interfaces;

public interface IApiKeyRepository
{
    Task<IEnumerable<string>> GetActiveApiKeysAsync();
    Task<bool> ValidateApiKeyAsync(string apiKey);
}