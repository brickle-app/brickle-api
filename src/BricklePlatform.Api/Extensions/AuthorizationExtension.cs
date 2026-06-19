using BricklePlatform.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Extensions;

public static class AuthorizationExtension
{
    public static IServiceCollection AddAuthorizationFilters(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ApiKeyAuthFilter>();
        
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.AddService<ApiKeyAuthFilter>();
        });
        
        return services;
    }
}