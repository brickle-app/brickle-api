using BricklePlatform.Api.Behaviors;
using MediatR;
using System.Reflection;

namespace BricklePlatform.Api.Extensions;

public static class MediatorExtension
{
    public static IServiceCollection AddMediatorExtension(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}