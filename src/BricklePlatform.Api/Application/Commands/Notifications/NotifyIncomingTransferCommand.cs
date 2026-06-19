using MediatR;

namespace BricklePlatform.Api.Application.Commands.Notifications;

public record NotifyIncomingTransferCommand(
    string SenderEmail,
    string RecipientWalletAddress,
    string Amount,
    string? TransactionHash
) : IRequest<NotifyIncomingTransferResult>;

public record NotifyIncomingTransferResult(bool Notified, string Message);
