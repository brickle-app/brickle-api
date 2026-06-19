using BricklePlatform.Infrastructure.Models;

namespace BricklePlatform.Infrastructure.Interfaces;

public interface IHttpClientService
{
    Task<Tuple<bool, string>> MakeRequestWithHeaders(RequestHttpModel request);
}