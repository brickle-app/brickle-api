using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;

namespace BricklePlatform.Api.Extensions;

public static class FluentValidationExtension
{
    public static IServiceCollection AddFluentValidationExtension(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}