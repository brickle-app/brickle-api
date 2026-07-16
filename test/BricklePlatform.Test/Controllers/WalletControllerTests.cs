using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BricklePlatform.Api.Controllers;
using BricklePlatform.Domain.DTOs.Wallet;
using BricklePlatform.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BricklePlatform.Test.Controllers;

public class WalletControllerTests
{
    [Fact]
    public async Task SaveBackupUsesAuthenticatedUserIdFromJwtSubject()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IWalletBackupService>();
        var request = ValidRequest();
        var response = ValidResponse();
        service.Setup(s => s.SaveAsync(userId, request)).ReturnsAsync(response);
        var controller = CreateController(service.Object, userId);

        var result = await controller.SaveBackup(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task GetBackupReturnsNotFoundWhenBackupDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IWalletBackupService>();
        service.Setup(s => s.GetAsync(userId)).ThrowsAsync(new KeyNotFoundException("Wallet backup not found"));
        var controller = CreateController(service.Object, userId);

        var result = await controller.GetBackup();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task SaveBackupReturnsBadRequestForInvalidWalletBackupPayload()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IWalletBackupService>();
        var request = ValidRequest();
        service.Setup(s => s.SaveAsync(userId, request)).ThrowsAsync(new InvalidOperationException("Plaintext private keys are not accepted"));
        var controller = CreateController(service.Object, userId);

        var result = await controller.SaveBackup(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpgradeBackupUsesAuthenticatedUserIdFromJwtSubject()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IWalletBackupService>();
        var request = ValidRequest();
        var response = ValidResponse();
        service.Setup(s => s.UpgradeActiveWalletAsync(userId, request)).ReturnsAsync(response);
        var controller = CreateController(service.Object, userId);

        var result = await controller.UpgradeBackup(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task UpgradeBackupUsesAuthenticatedUserIdFromMappedNameIdentifierClaim()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IWalletBackupService>();
        var request = ValidRequest();
        var response = ValidResponse();
        service.Setup(s => s.UpgradeActiveWalletAsync(userId, request)).ReturnsAsync(response);
        var controller = CreateController(service.Object, userId, ClaimTypes.NameIdentifier);

        var result = await controller.UpgradeBackup(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    private static WalletController CreateController(
        IWalletBackupService service,
        Guid userId,
        string claimType = JwtRegisteredClaimNames.Sub)
    {
        var controller = new WalletController(service, NullLogger<WalletController>.Instance);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(claimType, userId.ToString())
                }, "TestAuth"))
            }
        };
        return controller;
    }

    private static WalletBackupRequestDto ValidRequest() => new()
    {
        WalletAddress = "0x1111111111111111111111111111111111111111",
        EncryptedPrivateKey = "{\"crypto\":{}}",
        EncryptionVersion = "ethers-keystore-v1",
        Cipher = "ethers-json-keystore",
        Kdf = "scrypt",
        KdfParams = new Dictionary<string, object> { ["n"] = 131072 }
    };

    private static WalletBackupResponseDto ValidResponse() => new()
    {
        WalletAddress = "0x1111111111111111111111111111111111111111",
        EncryptedPrivateKey = "{\"crypto\":{}}",
        EncryptionVersion = "ethers-keystore-v1",
        Cipher = "ethers-json-keystore",
        Kdf = "scrypt",
        KdfParams = new Dictionary<string, object> { ["n"] = 131072 }
    };
}
