using BricklePlatform.Api.Application.Commands.UserDocument;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;
using DomainUser = BricklePlatform.Domain.Entities.User;
using BricklePlatform.Domain.Exceptions;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BricklePlatform.Api.Application.Handlers.UserDocument;

public class UpdateUserDocumentStatusCommandHandler : IRequestHandler<UpdateUserDocumentStatusCommand, UserDocumentDto>
{
    private readonly IUserDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UpdateUserDocumentStatusCommandHandler> _logger;

    public UpdateUserDocumentStatusCommandHandler(
        IUserDocumentRepository documentRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        IEmailService emailService,
        ILogger<UpdateUserDocumentStatusCommandHandler> logger)
    {
        _documentRepository = documentRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<UserDocumentDto> Handle(UpdateUserDocumentStatusCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id);
        if (document == null)
            throw new NotFoundException($"Document with ID {request.Id} not found");

        document.UpdateStatus(request.Status, request.Observation);
        await _documentRepository.UpdateAsync(document);

        if (request.Status == "APPROVED" || request.Status == "REJECTED")
        {
            var user = document.User;
            if (user == null && document.UserId.HasValue)
            {
                user = await _userRepository.GetByIdAsync(document.UserId.Value);
            }

            if (user != null)
            {
                user.IsBasicProfileComplete = request.Status == "APPROVED";
                user.IsFullProfileComplete = request.Status == "APPROVED";
                user.IsProfileUnderReview = false;
                await _userRepository.UpdateAsync(user);

                if (request.Status == "APPROVED")
                {
                    await TrySendProfileApprovedEmailAsync(user);
                    await TrySendProfileApprovedNotificationAsync(user, document.Id);
                }
                else
                {
                    await TrySendProfileRejectedEmailAsync(user, request.Observation);
                    await TrySendProfileRejectedNotificationAsync(user, document.Id, request.Observation);
                }
            }
        }

        return new UserDocumentDto
        {
            Id = document.Id,
            UserId = document.UserId,
            UserName = document.User != null ? $"{document.User.FirstName} {document.User.LastName}" : "Unknown",
            UserEmail = document.User?.Email ?? "Unknown",
            Name = document.Name,
            DocumentUrl = document.DocumentUrl,
            Status = document.Status,
            Observation = document.Observation,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }

    private async Task TrySendProfileApprovedEmailAsync(DomainUser user)
    {
        try
        {
            await _emailService.SendProfileApprovedAsync(user.Email, GetUserDisplayName(user));
            _logger.LogInformation("Email de perfil aprobado enviado al usuario {UserId}.", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "No se pudo enviar el email de perfil aprobado al usuario {UserId}. La aprobación se guardó correctamente.",
                user.Id);
        }
    }

    private async Task TrySendProfileRejectedEmailAsync(DomainUser user, string? observation)
    {
        try
        {
            await _emailService.SendProfileRejectedAsync(user.Email, GetUserDisplayName(user), observation);
            _logger.LogInformation("Email de perfil rechazado enviado al usuario {UserId}.", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "No se pudo enviar el email de perfil rechazado al usuario {UserId}. El rechazo se guardó correctamente.",
                user.Id);
        }
    }

    /// <summary>
    /// Envía push al usuario (Expo) cuando el admin aprueba el documento y el perfil queda verificado.
    /// No interrumpe el flujo si falla el envío o no hay token.
    /// </summary>
    private async Task TrySendProfileApprovedNotificationAsync(
        DomainUser user,
        Guid documentId)
    {
        if (string.IsNullOrWhiteSpace(user.PushNotificationToken))
        {
            _logger.LogInformation(
                "Perfil aprobado para usuario {UserId}: sin token push, omitiendo notificación.",
                user.Id);
            return;
        }

        try
        {
            var data = new Dictionary<string, object>
            {
                ["category"] = "PROFILE",
                ["type"] = "PROFILE_APPROVED",
                ["documentId"] = documentId.ToString("D"),
                ["userId"] = user.Id.ToString()
            };

            await _notificationService.SendNotificationAsync(
                user.PushNotificationToken,
                "Perfil verificado",
                "Tu identidad fue aprobada. Ya puedes usar todas las funciones de Brickle.",
                data);

            _logger.LogInformation(
                "Notificación de perfil aprobado enviada al usuario {UserId}.",
                user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "No se pudo enviar la notificación de perfil aprobado al usuario {UserId}. La aprobación se guardó correctamente.",
                user.Id);
        }
    }

    private async Task TrySendProfileRejectedNotificationAsync(
        DomainUser user,
        Guid documentId,
        string? observation)
    {
        if (string.IsNullOrWhiteSpace(user.PushNotificationToken))
        {
            _logger.LogInformation(
                "Perfil rechazado para usuario {UserId}: sin token push, omitiendo notificación.",
                user.Id);
            return;
        }

        try
        {
            var data = new Dictionary<string, object>
            {
                ["category"] = "PROFILE",
                ["type"] = "PROFILE_REJECTED",
                ["documentId"] = documentId.ToString("D"),
                ["userId"] = user.Id.ToString()
            };

            if (!string.IsNullOrWhiteSpace(observation))
            {
                data["observation"] = observation;
            }

            await _notificationService.SendNotificationAsync(
                user.PushNotificationToken,
                "Perfil rechazado",
                "Tu documento fue rechazado. Revisa el motivo y vuelve a cargarlo en Brickle.",
                data);

            _logger.LogInformation(
                "Notificación de perfil rechazado enviada al usuario {UserId}.",
                user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "No se pudo enviar la notificación de perfil rechazado al usuario {UserId}. El rechazo se guardó correctamente.",
                user.Id);
        }
    }

    private static string GetUserDisplayName(DomainUser user)
    {
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.Email : fullName;
    }
}
