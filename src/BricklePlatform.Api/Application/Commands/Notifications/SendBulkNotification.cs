using MediatR;

namespace BricklePlatform.Api.Application.Commands.Notifications;

public record SendBulkNotificationCommand(
  int BatchSize,
  string ActionId,
  string Title,
  string Body,
  object? Data
) : IRequest<bool>;