using System.Text.Json;
using BricklePlatform.Api.Controllers;
using BricklePlatform.Domain.DTOs.Relayer;
using BricklePlatform.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BricklePlatform.Test.Controllers;

public class RelayerControllerTests
{
    [Fact]
    public void RelayerControllerIsAuthorizedApiController()
    {
        var controllerType = typeof(RelayerController);

        Assert.NotNull(Attribute.GetCustomAttribute(controllerType, typeof(ApiControllerAttribute)));
        Assert.NotNull(Attribute.GetCustomAttribute(controllerType, typeof(AuthorizeAttribute)));
        var route = Assert.IsType<RouteAttribute>(Attribute.GetCustomAttribute(controllerType, typeof(RouteAttribute)));
        Assert.Equal("api/[controller]", route.Template);
    }

    [Fact]
    public async Task SponsorAcceptsDirectRelayerPayload()
    {
        var service = new Mock<IRelayerService>();
        service.Setup(s => s.SponsorAsync(It.IsAny<RelayerSponsorRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RelayerTransactionResponseDto { Status = true, Hash = "0xabc" });
        var controller = new RelayerController(service.Object, NullLogger<RelayerController>.Instance);
        using var document = JsonDocument.Parse("""
        {
          "command": "payment",
          "token": "0x0000000000000000000000000000000000000001",
          "sender": "0x0000000000000000000000000000000000000002",
          "leasingCore": "0x0000000000000000000000000000000000000003",
          "amount": "1000",
          "fee": "100000",
          "deadline": "9999999999",
          "permitSignature": { "v": 27, "r": "0xaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "s": "0xbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb" }
        }
        """);

        var result = await controller.Sponsor(document.RootElement, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RelayerTransactionResponseDto>(ok.Value);
        Assert.True(response.Status);
        service.Verify(s => s.SponsorAsync(It.Is<RelayerSponsorRequestDto>(r => r.Command == "payment" && r.Amount == "1000"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SponsorAcceptsDefenderStyleParamsPayload()
    {
        var service = new Mock<IRelayerService>();
        service.Setup(s => s.SponsorAsync(It.IsAny<RelayerSponsorRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RelayerTransactionResponseDto { Status = true, Hash = "0xdef" });
        var controller = new RelayerController(service.Object, NullLogger<RelayerController>.Instance);
        using var document = JsonDocument.Parse("""
        {
          "params": [
            {
              "command": "commit",
              "token": "0x0000000000000000000000000000000000000001",
              "sender": "0x0000000000000000000000000000000000000002",
              "campaign": "0x0000000000000000000000000000000000000003",
              "amount": "1000",
              "fee": "100000",
              "deadline": "9999999999",
              "permitSignature": { "v": 27, "r": "0xaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "s": "0xbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb" }
            }
          ]
        }
        """);

        var result = await controller.Sponsor(document.RootElement, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RelayerTransactionResponseDto>(ok.Value);
        Assert.True(response.Status);
        service.Verify(s => s.SponsorAsync(It.Is<RelayerSponsorRequestDto>(r => r.Command == "commit" && r.Campaign == "0x0000000000000000000000000000000000000003"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
