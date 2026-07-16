using System.Reflection;
using BricklePlatform.Infrastructure.Services;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Resend;
using Xunit;

namespace BricklePlatform.Test.Services;

public class EmailTemplateLogoTests
{
    private const string OfficialLogoUrl = "https://account.blob.core.windows.net/container/branding/email/brickle-logo-2026-07.png?sig=test";

    [Fact]
    public void BrandHeaderRendersOfficialPngWithCorrectProportion()
    {
        var html = BuildBrandHeaderRow(
            "https://account.blob.core.windows.net/container/branding/email/brickle-logo-2026-07.png?sig=test");

        Assert.Contains("<img", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("width=\"259\"", html);
        Assert.Contains("height=\"82\"", html);
        Assert.Contains("alt=\"Brickle - Donde crecer es más fácil\"", html);
        Assert.DoesNotContain("Inversión en activos reales", html);
        Assert.DoesNotContain("logo_green", html);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://brickle.app/old-logo.webp")]
    [InlineData("javascript:alert(1)")]
    public void BrandHeaderFallsBackToTextWithoutOldBranding(string? logoUrl)
    {
        var html = BuildBrandHeaderRow(logoUrl);

        Assert.DoesNotContain("<img", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(">Brickle<", html);
        Assert.DoesNotContain("#85FA8F", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#9B6FEB", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("logo_green", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AllNineTemplateGeneratorsIncludeTheConfiguredOfficialSharedHeader()
    {
        var settings = Options.Create(new InfrastructureSettings
        {
            EmailSettings = new EmailSettings
            {
                FromEmail = "sender@brickle.app",
                AdminEmail = "admin@brickle.app",
                LogoImageUrl = OfficialLogoUrl
            }
        });
        var service = new EmailService(
            Mock.Of<IResend>(),
            settings,
            NullLogger<EmailService>.Instance);
        var generators = typeof(EmailService)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(method => method.Name.StartsWith("Generate", StringComparison.Ordinal) && method.Name.EndsWith("Template", StringComparison.Ordinal))
            .OrderBy(method => method.Name)
            .ToArray();

        Assert.Equal(9, generators.Length);
        foreach (var generator in generators)
        {
            var arguments = generator.GetParameters().Select(CreateArgument).ToArray();
            var html = Assert.IsType<string>(generator.Invoke(service, arguments));
            Assert.Contains($"src=\"{OfficialLogoUrl.Replace("&", "&amp;")}\"", html);
            Assert.Contains("alt=\"Brickle - Donde crecer es más fácil\"", html);
            Assert.Contains("width=\"259\" height=\"82\"", html);
        }
    }

    private static string BuildBrandHeaderRow(string? logoImageUrl)
    {
        var method = typeof(EmailService).GetMethod("BuildBrandHeaderRow", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (string)method.Invoke(null, new object?[] { logoImageUrl })!;
    }

    private static object? CreateArgument(ParameterInfo parameter) => parameter.ParameterType switch
    {
        var type when type == typeof(string) => "test-value",
        var type when type == typeof(decimal) => 123.45m,
        _ => throw new InvalidOperationException($"No test argument configured for {parameter.ParameterType}.")
    };
}
