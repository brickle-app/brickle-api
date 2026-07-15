using System.Reflection;
using BricklePlatform.Infrastructure.Services;
using Xunit;

namespace BricklePlatform.Test.Services;

public class EmailTemplateLogoTests
{
    [Fact]
    public void BrandHeaderUsesCrispHtmlWordmarkForWebpLogoUrls()
    {
        var html = BuildBrandHeaderRow("https://brickle.app/assets/logo_green-B0JL5kO0.webp");

        Assert.DoesNotContain("<img", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Brickle", html);
        Assert.Contains("Inversión en activos reales", html);
    }

    [Fact]
    public void BrandHeaderAllowsPngLogoUrlsWhenConfigured()
    {
        var html = BuildBrandHeaderRow("https://cdn.brickle.app/email/logo@2x.png");

        Assert.Contains("<img", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("width=\"200\"", html);
        Assert.Contains("height=\"38\"", html);
    }

    private static string BuildBrandHeaderRow(string? logoImageUrl)
    {
        var method = typeof(EmailService).GetMethod("BuildBrandHeaderRow", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (string)method.Invoke(null, new object?[] { logoImageUrl })!;
    }
}
