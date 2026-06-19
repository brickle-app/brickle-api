using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Api.Application.Commands.Notifications;
using MediatR;
using BricklePlatform.Domain.Exceptions;

namespace BricklePlatform.Api.Application.Handlers.Notifications
{
  public class SendNotificationHandler : IRequestHandler<SendNotificationCommand, bool>
  {
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SendNotificationHandler> _logger;

    public SendNotificationHandler(INotificationService notificationService, IUserRepository userRepository, ILogger<SendNotificationHandler> logger)
    {
      _notificationService = notificationService;
      _userRepository = userRepository;
      _logger = logger;
    }

    public async Task<bool> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {

      Domain.Entities.User? usuario = await _userRepository.GetByIdAsync(Guid.Parse(request.UserId));
      if (usuario == null)
      {
        throw new NotFoundException($"Usuario con ID {request.UserId} no encontrado");
      }

      if (!string.IsNullOrEmpty(usuario.PushNotificationToken))
      {
        try
        {
          await _notificationService.SendNotificationAsync(
              usuario.PushNotificationToken,
              "Acción Completada",
              $"La acción solicitada se ha completado exitosamente.",
              new { actionId = request.ActionId, timestamp = DateTime.UtcNow }
          );
          return true;
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error al enviar notificación al usuario {UserId}", request.UserId);
          return false;
        }
      }

      return false;
    }
  }
}