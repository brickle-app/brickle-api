using BricklePlatform.Api.Application.ExceptionHandler;
using BricklePlatform.Api.Middleware;
using BricklePlatform.Infrastructure;
using BricklePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace BricklePlatform.Api.Extensions;

public static class WebApplicationExtension
{
    public static WebApplication CreateWebApplication(this WebApplicationBuilder builder)
    {
        string? env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables();

        // Servicios
        builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = false;
        });
        builder.Services.AddFluentValidationExtension();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddMediatorExtension();
        builder.Services.AddSwaggerExtension();
        builder.Services.AddCorsExtension();
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddAuthServices(builder.Configuration);
        builder.Services.AddApplicationInsightsTelemetry();
        builder.Services.AddApplicationInsightsKubernetesEnricher();

        // Configuración ApplicationInsights
        builder.Logging.AddApplicationInsights();
        LogLevel defaultLogLevel = builder.Configuration.GetValue("InfrastructureSettings:LoggingSettings:LogLevelSettings:Default", LogLevel.Warning);
        LogLevel apiLogLevel = builder.Configuration.GetValue("InfrastructureSettings:LoggingSettings:LogLevelSettings:Api", LogLevel.Information);

        builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("", defaultLogLevel);
        builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("InfrastructureSettings", apiLogLevel);

        return builder.Build();
    }

    public static WebApplication ConfigureWebApplication(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.UseSwagger();

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "BricklePlatform API v1");
            c.RoutePrefix = string.Empty;
        });

        app.UseCors("mycors");
        app.UseHttpsRedirection();

        app.UseMiddleware<ErrorHandlerMiddleware>(new Dictionary<Type, IExceptionHandler>
        {
            {typeof(ArgumentException), new ArgumentExceptionHandler() },
            {typeof(ApplicationException), new ApplicationExceptionHandler() }
        });

        // Apply migrations
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();

            // Manual check for is_profile_under_review if EF sync failed
            string sqlCheck = @"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[User]') 
                    AND name = 'is_profile_under_review'
                )
                BEGIN
                    ALTER TABLE [dbo].[User] ADD [is_profile_under_review] bit NOT NULL DEFAULT 0;
                END";

            try
            {
                dbContext.Database.ExecuteSqlRaw(sqlCheck);
            }
            catch (Exception ex)
            {
                // Log or handle error if needed, but don't block startup if it's just a permission issue or already exists unexpectedly
                app.Logger.LogWarning(ex, "Attempted to manually add is_profile_under_review column but failed.");
            }
        }

        return app;
    }
}