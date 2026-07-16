using BricklePlatform.Api.Controllers;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BricklePlatform.Test.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task VerifyOtpCreatesNewEmailUserWithIncompleteBasicProfile()
    {
        var jwtService = new Mock<IJwtService>();
        var userRepository = new Mock<IUserRepository>();
        var userService = new Mock<IUserService>();
        var emailService = new Mock<IEmailService>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var email = "hijap71603@meikeya.com";
        cache.Set($"otp_{email}", "123456", TimeSpan.FromMinutes(5));
        jwtService.Setup(s => s.GenerateAccessToken(It.IsAny<Guid>(), email)).Returns("access-token");
        jwtService.Setup(s => s.GenerateRefreshToken()).Returns("refresh-token");
        userRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((User?)null);
        userRepository.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User user) => user);
        var controller = new AuthController(
            jwtService.Object,
            userRepository.Object,
            userService.Object,
            emailService.Object,
            cache,
            NullLogger<AuthController>.Instance);

        var result = await controller.VerifyOtp(new VerifyOtpRequest { Email = email, Otp = "123456" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.False(response.User.IsBasicProfileComplete);
        Assert.False(response.User.IsFullProfileComplete);
        Assert.Equal(string.Empty, response.User.PhoneNumber);
        userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task VerifyOtpReturnsIncompleteBasicProfileForExistingUserWithStaleCompleteFlag()
    {
        var jwtService = new Mock<IJwtService>();
        var userRepository = new Mock<IUserRepository>();
        var userService = new Mock<IUserService>();
        var emailService = new Mock<IEmailService>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var email = "hijap71603@meikeya.com";
        var user = User.Create(
            firstName: "hijap71603",
            lastName: "",
            email: email,
            phoneNumber: "",
            termsAccepted: true,
            passwordHash: Array.Empty<byte>(),
            passwordSalt: Array.Empty<byte>());
        user.Update(isBasicProfileComplete: true);
        cache.Set($"otp_{email}", "123456", TimeSpan.FromMinutes(5));
        jwtService.Setup(s => s.GenerateAccessToken(user.Id, email)).Returns("access-token");
        jwtService.Setup(s => s.GenerateRefreshToken()).Returns("refresh-token");
        userRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);
        var controller = new AuthController(
            jwtService.Object,
            userRepository.Object,
            userService.Object,
            emailService.Object,
            cache,
            NullLogger<AuthController>.Instance);

        var result = await controller.VerifyOtp(new VerifyOtpRequest { Email = email, Otp = "123456" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.False(response.User.IsBasicProfileComplete);
        userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
}
