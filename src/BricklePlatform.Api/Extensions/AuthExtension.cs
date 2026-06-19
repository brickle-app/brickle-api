using System.Text;
using BricklePlatform.Infrastructure.Services;
using BricklePlatform.Infrastructure.Settings;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BricklePlatform.Api.Extensions;

public static class AuthExtension
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        var infrastructureSettings = configuration.GetSection("InfrastructureSettings").Get<InfrastructureSettings>();
        if (infrastructureSettings == null)
            throw new InvalidOperationException("InfrastructureSettings configuration is missing");

        var jwtSettings = infrastructureSettings.JwtSettings;
        var firebaseSettings = infrastructureSettings.FirebaseSettings;

        // Firebase Admin SDK
        InitializeFirebase(firebaseSettings);

        // JWT Bearer authentication
        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }

    private static void InitializeFirebase(FirebaseSettings settings)
    {
        if (FirebaseApp.DefaultInstance != null)
            return;

        var credentialPath = settings.CredentialsFilePath;

        // Try FIREBASE_CREDENTIALS_JSON first (Azure-friendly: inline JSON → temp file)
        var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
        if (!string.IsNullOrEmpty(firebaseJson))
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"firebase-credentials-{Guid.NewGuid()}.json");
            File.WriteAllText(tempFile, firebaseJson);
            credentialPath = tempFile;
        }

        // Fallback to GOOGLE_APPLICATION_CREDENTIALS (file path, not inline JSON)
        if (string.IsNullOrEmpty(credentialPath))
            credentialPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

        GoogleCredential credential;
        if (!string.IsNullOrEmpty(credentialPath))
            credential = GoogleCredential.FromFile(credentialPath);
        else
            credential = GoogleCredential.GetApplicationDefault();

        FirebaseApp.Create(new AppOptions
        {
            Credential = credential,
            ProjectId = settings.ProjectId
        });
    }
}
