using BricklePlatform.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace BricklePlatform.Api.Filters
{
    public class ApiKeyAuthFilter : IAsyncAuthorizationFilter
    {
        private readonly ILogger<ApiKeyAuthFilter> _logger;
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private const string API_KEYS_CACHE_KEY = "ActiveApiKeys";
        private const int CACHE_DURATION_MINUTES = 20;
        private const string API_KEY_HEADER_NAME = "api-key";

        private const string UNAUTHORIZED_MESSAGE_PROD = "Unauthorized";
        private const string API_KEY_MISSING_MESSAGE = "La consulta falló porque no se proporcionó una API key";
        private const string API_KEY_INVALID_MESSAGE = "La consulta falló porque la API Key proporcionada no es válida";

        public ApiKeyAuthFilter(
            ILogger<ApiKeyAuthFilter> logger,
            IApiKeyRepository apiKeyRepository,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _logger = logger;
            _apiKeyRepository = apiKeyRepository;
            _cache = cache;
            _configuration = configuration;
        }

        private async Task<IEnumerable<string>> GetActiveApiKeysAsync()
        {
            return await _cache.GetOrCreateAsync(API_KEYS_CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES);
                return await _apiKeyRepository.GetActiveApiKeysAsync();
            }) ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Validación de api-key para autenticación
        /// </summary>
        /// <param name="context">Contexto de autorización</param>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            string? environment = _configuration["ASPNETCORE_ENVIRONMENT"];
            bool isProduction = environment?.Equals("Production", StringComparison.OrdinalIgnoreCase) ?? false;

            if (!context.HttpContext.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out StringValues apiKey))
            {
                HandleError(context, API_KEY_MISSING_MESSAGE, isProduction);
                return;
            }

            string apiKeyString = apiKey.ToString();
            IEnumerable<string> activeKeys = await GetActiveApiKeysAsync();

            if (!activeKeys.Contains(apiKeyString))
            {
                string detailedMessage = $"{API_KEY_INVALID_MESSAGE}. Api-key: {apiKeyString}";
                _logger.LogError(detailedMessage);
                HandleError(context, API_KEY_INVALID_MESSAGE, isProduction);
                return;
            }
        }

        private void HandleError(AuthorizationFilterContext context, string message, bool isProduction)
        {
            _logger.LogError(message);
            string responseMessage = isProduction ? UNAUTHORIZED_MESSAGE_PROD : message;
            context.Result = new UnauthorizedObjectResult(new
            {
                message = responseMessage,
                timestamp = DateTime.UtcNow,
                status = StatusCodes.Status401Unauthorized
            });
        }
    }
}