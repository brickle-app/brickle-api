using BricklePlatform.Api.Application.Commands.UserDocument;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;
using DomainUser = BricklePlatform.Domain.Entities.User;
using BricklePlatform.Domain.Exceptions;
using BricklePlatform.Domain.Interfaces;
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
    private readonly ILogger<UpdateUserDocumentStatusCommandHandler> _logger;

    public UpdateUserDocumentStatusCommandHandler(
        IUserDocumentRepository documentRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        ILogger<UpdateUserDocumentStatusCommandHandler> logger)
    {
        _documentRepository = documentRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<UserDocumentDto> Handle(UpdateUserDocumentStatusCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id);
        if (document == null)
            throw new NotFoundException($"Document with ID {request.Id} not found");

        document.UpdateStatus(request.Status, request.Observation);
        await _documentRepository.UpdateAsync(document);

        if (request.Status == "APPROVED")
        {
            var user = document.User;
            if (user == null && document.UserId.HasValue)
            {
                user = await _userRepository.GetByIdAsync(document.UserId.Value);
            }

            if (user != null)
            {
                user.IsBasicProfileComplete = true;
                user.IsFullProfileComplete = true;
                user.IsProfileUnderReview = false;
                await _userRepository.UpdateAsync(user);

                await TrySendProfileApprovedNotificationAsync(user, document.Id);
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
}
