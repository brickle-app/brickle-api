using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.DTOs.Relayer;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Domain.Models;
using BricklePlatform.Infrastructure.Interfaces;
using BricklePlatform.Infrastructure.Services;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BricklePlatform.Test.Services;

public class WebHookServiceDelegationTests
{
    [Fact]
    public async Task ProcessPaymentWebhookAsyncDelegatesToRelayerService()
    {
        var relayer = new Mock<IRelayerService>();
        relayer.Setup(x => x.SponsorPaymentAsync(It.IsAny<PaymentDto>(), "0xsender", "0xleasing", "0xtoken", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RelayerTransactionResponseDto { Hash = "0xhash", Status = true });

        var service = CreateService(relayer.Object);

        var result = await service.ProcessPaymentWebhookAsync(new PaymentDto { PaymentAmount = "1000", Deadline = 1, PermitSignature = ValidSignature() }, "0xsender", "0xleasing", "0xtoken");

        Assert.True(result.Status);
        Assert.Equal("0xhash", result.Hash);
        relayer.Verify(x => x.SponsorPaymentAsync(It.IsAny<PaymentDto>(), "0xsender", "0xleasing", "0xtoken", It.IsAny<CancellationToken>()), Times.Once);
    }

    private static WebHookService CreateService(IRelayerService relayerService)
    {
        var http = new Mock<IHttpClientService>();
        var settings = Options.Create(new InfrastructureSettings());
        return new WebHookService(NullLogger<WebHookService>.Instance, http.Object, settings, relayerService);
    }

    private static PermitSignatureDto ValidSignature() => new()
    {
        V = 27,
        R = "0x" + new string('a', 64),
        S = "0x" + new string('b', 64)
    };
}
