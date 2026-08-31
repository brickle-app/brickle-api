using BricklePlatform.Api.Application.Commands.UserDocument;
using BricklePlatform.Api.Application.Handlers.UserDocument;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using DomainUser = BricklePlatform.Domain.Entities.User;
using DomainUserDocument = BricklePlatform.Domain.Entities.UserDocument;

namespace BricklePlatform.Test.Handlers.UserDocument;

public class UpdateUserDocumentStatusCommandHandlerTests
{
    [Fact]
    public async Task ApprovedDocumentSendsApprovalEmailAndPushNotificationWhenAllRequiredDocumentsApproved()
    {
        var user = CreateUser();
        var document = DomainUserDocument.Create(user.Id, "Identity Document", UserDocumentType.Identity, "https://example.com/id.png");
        document.UpdateStatus("APPROVED");
        var bankCertificate = DomainUserDocument.Create(user.Id, "Bank Certificate", UserDocumentType.BankCertificate, "https://example.com/bank.png");
        bankCertificate.UpdateStatus("APPROVED");
        var documentRepository = new Mock<IUserDocumentRepository>();
        var userRepository = new Mock<IUserRepository>();
        var notificationService = new Mock<INotificationService>();
        var emailService = new Mock<IEmailService>();
        documentRepository.Setup(r => r.GetByIdAsync(document.Id)).ReturnsAsync(document);
        documentRepository.Setup(r => r.UpdateAsync(document)).ReturnsAsync(document);
        documentRepository.Setup(r => r.GetByUserIdAsync(user.Id))
            .ReturnsAsync(new[] { document, bankCertificate });
        userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var handler = new UpdateUserDocumentStatusCommandHandler(
            documentRepository.Object,
            userRepository.Object,
            notificationService.Object,
            emailService.Object,
            NullLogger<UpdateUserDocumentStatusCommandHandler>.Instance);

        await handler.Handle(new UpdateUserDocumentStatusCommand(document.Id, "APPROVED"), CancellationToken.None);

        emailService.Verify(e => e.SendProfileApprovedAsync(user.Email, "Santiago Garcia"), Times.Once);
        notificationService.Verify(n => n.SendNotificationAsync(
            user.PushNotificationToken!,
            "Perfil verificado",
            It.Is<string>(body => body.Contains("aprobada", StringComparison.OrdinalIgnoreCase)),
            It.Is<object>(data => HasNotificationType(data, "PROFILE_APPROVED"))), Times.Once);
    }

    [Fact]
    public async Task ApprovedDocumentDoesNotSendApprovalWhenOtherRequiredDocumentIsMissing()
    {
        var user = CreateUser();
        var document = DomainUserDocument.Create(user.Id, "Identity Document", UserDocumentType.Identity, "https://example.com/id.png");
        document.UpdateStatus("APPROVED");
        var documentRepository = new Mock<IUserDocumentRepository>();
        var userRepository = new Mock<IUserRepository>();
        var notificationService = new Mock<INotificationService>();
        var emailService = new Mock<IEmailService>();
        documentRepository.Setup(r => r.GetByIdAsync(document.Id)).ReturnsAsync(document);
        documentRepository.Setup(r => r.UpdateAsync(document)).ReturnsAsync(document);
        documentRepository.Setup(r => r.GetByUserIdAsync(user.Id))
            .ReturnsAsync(new[] { document });
        userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var handler = new UpdateUserDocumentStatusCommandHandler(
            documentRepository.Object,
            userRepository.Object,
            notificationService.Object,
            emailService.Object,
            NullLogger<UpdateUserDocumentStatusCommandHandler>.Instance);

        await handler.Handle(new UpdateUserDocumentStatusCommand(document.Id, "APPROVED"), CancellationToken.None);

        emailService.Verify(e => e.SendProfileApprovedAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        notificationService.Verify(n => n.SendNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task RejectedDocumentSendsRejectionEmailAndPushNotification()
    {
        var user = CreateUser();
        var document = DomainUserDocument.Create(user.Id, "Identity Document", UserDocumentType.Identity, "https://example.com/id.png");
        var documentRepository = new Mock<IUserDocumentRepository>();
        var userRepository = new Mock<IUserRepository>();
        var notificationService = new Mock<INotificationService>();
        var emailService = new Mock<IEmailService>();
        documentRepository.Setup(r => r.GetByIdAsync(document.Id)).ReturnsAsync(document);
        documentRepository.Setup(r => r.UpdateAsync(document)).ReturnsAsync(document);
        userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var handler = new UpdateUserDocumentStatusCommandHandler(
            documentRepository.Object,
            userRepository.Object,
            notificationService.Object,
            emailService.Object,
            NullLogger<UpdateUserDocumentStatusCommandHandler>.Instance);

        await handler.Handle(new UpdateUserDocumentStatusCommand(document.Id, "REJECTED", "Documento ilegible"), CancellationToken.None);

        emailService.Verify(e => e.SendProfileRejectedAsync(user.Email, "Santiago Garcia", "Documento ilegible"), Times.Once);
        notificationService.Verify(n => n.SendNotificationAsync(
            user.PushNotificationToken!,
            "Perfil rechazado",
            It.Is<string>(body => body.Contains("rechazado", StringComparison.OrdinalIgnoreCase)),
            It.Is<object>(data => HasNotificationType(data, "PROFILE_REJECTED"))), Times.Once);
    }

    private static DomainUser CreateUser()
    {
        return DomainUser.Create(
            firstName: "Santiago",
            lastName: "Garcia",
            email: "santiago@example.com",
            phoneNumber: "3000000000",
            termsAccepted: true,
            passwordHash: Array.Empty<byte>(),
            passwordSalt: Array.Empty<byte>(),
            pushNotificationToken: "ExponentPushToken[test]");
    }

    private static bool HasNotificationType(object data, string expectedType)
    {
        return data is IDictionary<string, object> dictionary &&
            dictionary.TryGetValue("type", out var type) &&
            type?.ToString() == expectedType;
    }
}
