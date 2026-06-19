using MediatR;

namespace BricklePlatform.Api.Application.Commands.Notifications;

public record SendNotificationCommand(
  string UserId,
  string ActionId,
  object? Data
) : IRequest<bool>;