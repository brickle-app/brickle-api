using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Exceptions;
using BricklePlatform.Infrastructure.Interfaces;
using BricklePlatform.Infrastructure.Persistence;
using BricklePlatform.Infrastructure.Repositories;
using BricklePlatform.Infrastructure.Services;
using BricklePlatform.Infrastructure.Services.Base;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Resend;

namespace BricklePlatform.Infrastructure;

public static class DependencyContainer
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InfrastructureSettings>(configuration.GetSection("InfrastructureSettings"));
        services.Configure<ExpoSettings>(configuration.GetSection("InfrastructureSettings:ExpoSettings"));
        services.Configure<EmailSettings>(configuration.GetSection("InfrastructureSettings:EmailSettings"));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetSection("InfrastructureSettings:DatabaseSettings:ConnectionString").Value));

        // Repositories
        services.AddTransient<IApiKeyRepository, ApiKeyRepository>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<ICompanyRepository, CompanyRepository>();
        services.AddTransient<IUserContactRepository, UserContactRepository>();
        services.AddTransient<IUserLeasingAgreementRepository, UserLeasingAgreementRepository>();
        services.AddTransient<ILeasingRepository, LeasingRepository>();
        services.AddTransient<IBlobStorageRepository, BlobStorageRepository>();
        services.AddTransient<ILogRepository, LogRepository>();
        services.AddTransient<IUserActivityLogRepository, UserActivityLogRepository>();
        services.AddTransient<ICampaignRepository, CampaignRepository>();
        services.AddTransient<IInvestmentRepository, InvestmentRepository>();
        services.AddTransient<IUserBankAccountRepository, UserBankAccountRepository>();
        services.AddTransient<IUserDocumentRepository, UserDocumentRepository>();
        services.AddTransient<IWalletBackupRepository, WalletBackupRepository>();

        // Services
        services.AddHttpClient();
        services.AddTransient<IHttpClientService, HttpClientService>();

        services.AddTransient<IWeb3Service, Web3Service>();
        services.AddTransient<IPasswordService, PasswordService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<ILeasingService, LeasingService>();
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<IEntityFileUpdater, LeasingFileUpdater>();
        services.AddTransient<LeasingFileUpdater>();
        services.AddTransient<UserFileUpdater>();
        services.AddTransient<PaymentFileUpdater>();
        services.AddTransient<IRelayerService, RelayerService>();
        services.AddTransient<IWebHookService, WebHookService>();
        services.AddTransient<INotificationService, ExpoNotificationService>();
        services.AddTransient<IThresholdFactoryService, ThresholdFactoryService>();
        services.AddTransient<ILeasingCoreService, LeasingCoreService>();
        services.AddTransient<IUserActivityLogService, UserActivityLogService>();
        services.AddTransient<IWalletBackupService, WalletBackupService>();

        services.AddMemoryCache();
        services.AddTransient<IJwtService, JwtService>();

        // Email Service configuration
        services.Configure<ResendClientOptions>(options =>
        {
            var emailSettings = configuration.GetSection("InfrastructureSettings:EmailSettings").Get<EmailSettings>();
            if (emailSettings == null || string.IsNullOrEmpty(emailSettings.ApiKey))
            {
                throw new InfrastructureException("Email configuration is missing or API key is not set");
            }
            options.ApiToken = emailSettings.ApiKey;
        });
        services.AddScoped<IResend, ResendClient>();
        services.AddTransient<IEmailService, EmailService>();

        ConfigureHttpClient(services, configuration);

        return services;
    }

    private static void ConfigureHttpClient(IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection configSection = configuration.GetSection("InfrastructureSettings");
        InfrastructureSettings? settings = configSection.Get<InfrastructureSettings>();

        if (settings?.HttpClientSettings == null)
        {
            throw new InfrastructureException("La configuración HttpClientSettings no está definida");
        }

        int maxRetry = settings.HttpClientSettings.MaxRetries > 0 ? settings.HttpClientSettings.MaxRetries : 3;
        int secondsWait = settings.HttpClientSettings.RetryDelaySeconds > 0 ? settings.HttpClientSettings.RetryDelaySeconds : 2;
        int timeOut = settings.HttpClientSettings.TimeoutSeconds > 0 ? settings.HttpClientSettings.TimeoutSeconds : 30;

        services.AddHttpClient("Webhook")
            .AddPolicyHandler(GetRetryPolicy(maxRetry, secondsWait))
            .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(timeOut));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int maxRetry, int secondsWait) =>
        HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(maxRetry, retryAttempt => TimeSpan.FromSeconds(secondsWait * retryAttempt),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine($"--- Reintentado comunicación desde HttpClient, reintento # {retryAttempt}, Detalle: {outcome.Result}");
                }
            );
}
