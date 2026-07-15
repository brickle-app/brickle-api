using System.Text.Encodings.Web;
using BricklePlatform.Api.Authentication;
using BricklePlatform.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BricklePlatform.Test.Auth;

public class ApiKeyAuthenticationHandlerTests
{
    [Fact]
    public async Task AuthenticateAsyncSucceedsWhenApiKeyHeaderIsValid()
    {
        var repository = new Mock<IApiKeyRepository>();
        repository.Setup(r => r.ValidateApiKeyAsync("valid-key")).ReturnsAsync(true);

        var handler = CreateHandler(repository.Object, "valid-key");

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.Equal("api-key", result.Principal?.Identity?.AuthenticationType);
    }

    [Fact]
    public async Task AuthenticateAsyncReturnsNoResultWhenApiKeyHeaderIsMissing()
    {
        var repository = new Mock<IApiKeyRepository>();
        var handler = CreateHandler(repository.Object, null);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.True(result.None);
    }

    private static ApiKeyAuthenticationHandler CreateHandler(IApiKeyRepository repository, string? apiKey)
    {
        var options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Setup(o => o.Get(It.IsAny<string>())).Returns(new AuthenticationSchemeOptions());

        var handler = new ApiKeyAuthenticationHandler(
            options.Object,
            NullLoggerFactory.Instance,
            UrlEncoder.Default,
            repository);

        var context = new DefaultHttpContext();
        if (apiKey != null)
        {
            context.Request.Headers[ApiKeyAuthenticationHandler.HeaderName] = apiKey;
        }

        handler.InitializeAsync(
            new AuthenticationScheme(ApiKeyAuthenticationHandler.SchemeName, null, typeof(ApiKeyAuthenticationHandler)),
            context).GetAwaiter().GetResult();

        return handler;
    }
}
