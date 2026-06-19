using BricklePlatform.Api.Application.Commands.Notifications;
using BricklePlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BricklePlatform.Api.Application.Handlers.Notifications;

public class NotifyIncomingTransferHandler
    : IRequestHandler<NotifyIncomingTransferCommand, NotifyIncomingTransferResult>
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotifyIncomingTransferHandler> _logger;

    public NotifyIncomingTransferHandler(
        IUserRepository userRepository,
        INotificationService notificationService,
        ILogger<NotifyIncomingTransferHandler> logger)
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<NotifyIncomingTransferResult> Handle(
        NotifyIncomingTransferCommand request,
        CancellationToken cancellationToken)
    {
        var sender = await _userRepository.GetByEmailAsync(request.SenderEmail.Trim());
        if (sender == null)
        {
            _logger.LogWarning(
                "NotifyIncomingTransfer: remitente no encontrado por email {Email}",
                request.SenderEmail);
            return new NotifyIncomingTransferResult(false, "Remitente no encontrado.");
        }

        var recipient = await _userRepository.GetByWalletAddressAsync(
            request.RecipientWalletAddress.Trim());
        if (recipient == null)
        {
            _logger.LogInformation(
                "NotifyIncomingTransfer: destinatario no registrado para wallet {Wallet}",
                request.RecipientWalletAddress);
            return new NotifyIncomingTransferResult(
                false,
                "El destinatario no tiene cuenta Brickle; no se envía notificación.");
        }

        if (recipient.Id == sender.Id)
        {
            return new NotifyIncomingTransferResult(false, "Mismo usuario; omitido.");
        }

        if (string.IsNullOrWhiteSpace(recipient.PushNotificationToken))
        {
            _logger.LogInformation(
                "NotifyIncomingTransfer: usuario {UserId} sin token push.",
                recipient.Id);
            return new NotifyIncomingTransferResult(
                false,
                "Destinatario sin token de notificaciones.");
        }

        var senderName = $"{sender.FirstName} {sender.LastName}".Trim();
        if (string.IsNullOrEmpty(senderName))
            senderName = "Un usuario Brickle";

        var amountDisplay = request.Amount.Trim();
        var body =
            $"{senderName} te envió saldo por {amountDisplay}. Ya está disponible en tu billetera.";

        var data = new Dictionary<string, object>
        {
            ["category"] = "MOVEMENT",
            ["type"] = "INCOMING_PEER_TRANSFER",
            ["amount"] = amountDisplay,
            ["senderId"] = sender.Id.ToString(),
            ["recipientId"] = recipient.Id.ToString(),
        };

        if (!string.IsNullOrWhiteSpace(request.TransactionHash))
            data["transactionHash"] = request.TransactionHash!;

        try
        {
            await _notificationService.SendNotificationAsync(
                recipient.PushNotificationToken,
                "Recibiste saldo",
                body,
                data);

            _logger.LogInformation(
                "NotifyIncomingTransfer: notificación enviada a {RecipientId} desde {SenderId}",
                recipient.Id,
                sender.Id);

            return new NotifyIncomingTransferResult(true, "Notificación enviada.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "NotifyIncomingTransfer: falló Expo para destinatario {RecipientId}",
                recipient.Id);
            return new NotifyIncomingTransferResult(
                false,
                "No se pudo enviar la notificación push.");
        }
    }
}
