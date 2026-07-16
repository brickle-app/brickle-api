using BricklePlatform.Api.Controllers;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BricklePlatform.Test.Controllers;

public class UserControllerTests
{
    [Fact]
    public async Task GetUserIncludesProfileUnderReviewFlag()
    {
        var user = VerifiedUserUnderReview();
        var repository = new Mock<IUserRepository>();
        repository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var controller = CreateController(repository.Object);

        var result = await controller.GetUser(Header(), user.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<UserDto>(ok.Value);
        Assert.True(dto.IsProfileUnderReview);
    }

    [Fact]
    public async Task GetUserByEmailIncludesProfileUnderReviewFlag()
    {
        var user = VerifiedUserUnderReview();
        var repository = new Mock<IUserRepository>();
        repository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        var controller = CreateController(repository.Object);

        var result = await controller.GetUserByEmail(Header(), user.Email);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<UserDto>(ok.Value);
        Assert.True(dto.IsProfileUnderReview);
    }

    private static UserController CreateController(IUserRepository userRepository)
    {
        return new UserController(
            Mock.Of<IUserService>(),
            userRepository,
            Mock.Of<IUserBankAccountRepository>(),
            NullLogger<UserController>.Instance,
            Mock.Of<IMediator>(),
            Mock.Of<IUserActivityLogService>(),
            Mock.Of<IEmailService>(),
            Mock.Of<INotificationService>());
    }

    private static User VerifiedUserUnderReview()
    {
        var user = User.Create(
            "Juan",
            "Perez",
            "jfvs0998@gmail.com",
            "3001234567",
            true,
            Array.Empty<byte>(),
            Array.Empty<byte>(),
            dateOfBirth: new DateTime(1990, 1, 1),
            nationality: "CO",
            countryOfResidence: "CO",
            documentType: BricklePlatform.Domain.Enums.DocumentTypeEnum.CC,
            documentNumber: "1023456856");
        user.IsBasicProfileComplete = true;
        user.IsProfileUnderReview = true;
        return user;
    }

    private static HeaderRequestModel Header() => new()
    {
        CorrelationId = Guid.NewGuid().ToString(),
        User = "jfvs0998@gmail.com",
        Source = "test",
        RequestDate = DateTime.UtcNow
    };
}
