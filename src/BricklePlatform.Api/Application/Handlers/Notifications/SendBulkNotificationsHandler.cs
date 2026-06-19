using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Api.Application.Commands.Notifications;
using MediatR;
using BricklePlatform.Domain.DTOs;

namespace BricklePlatform.Api.Application.Handlers.Notifications
{
  public class SendBulkNotificationsHandler : IRequestHandler<SendBulkNotificationCommand, bool>
  {
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SendBulkNotificationsHandler> _logger;

    public SendBulkNotificationsHandler(
        INotificationService notificationService,
        IUserRepository userRepository,
        ILogger<SendBulkNotificationsHandler> logger)
    {
      _notificationService = notificationService;
      _userRepository = userRepository;
      _logger = logger;
    }

    public async Task<bool> Handle(SendBulkNotificationCommand request, CancellationToken cancellationToken)
    {
      _logger.LogInformation("Iniciando envío masivo de notificaciones. Título: {Title}", request.Title);

      try
      {
        int pageNumber = 1;
        List<Domain.Entities.User> userBatch;
        int totalProcessed = 0;
        int totalErrors = 0;

        do
        {

          userBatch = await _userRepository.GetUsersWithTokensAsync(pageNumber, request.BatchSize, cancellationToken);
          _logger.LogDebug("Obtenido lote {PageNumber} con {Count} usuarios.", pageNumber, userBatch.Count);

          if (userBatch.Any())
          {
            var (processedInBatch, errorsInBatch) = await ProcessBatchAsync(userBatch, request);
            totalProcessed += processedInBatch;
            totalErrors += errorsInBatch;
            pageNumber++;
          }

        } while (userBatch.Count == request.BatchSize);

        _logger.LogInformation(
            "Envío masivo de notificaciones finalizado. Total procesados: {TotalProcessed}, Errores: {TotalErrors}. Título: {Title}",
            totalProcessed, totalErrors, request.Title);

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error crítico durante el envío masivo de notificaciones. Título: {Title}", request.Title);

        throw;
      }
    }

    private async Task<(int Processed, int Errors)> ProcessBatchAsync(
        IEnumerable<Domain.Entities.User> userBatch,
        SendBulkNotificationCommand request)
    {
      int processed = 0;
      int errors = 0;

      var notificationRequests = userBatch
          .Where(u => !string.IsNullOrEmpty(u.PushNotificationToken))
          .Select(user => new NotificationDto
          {
            RecipientId = user.PushNotificationToken ?? string.Empty,
            Title = request.Title,
            Body = request.Body,
            Data = request.Data
          })
          .ToList();

      if (notificationRequests.Count > 0)
      {
        try
        {
          _logger.LogDebug("Enviando lote de {Count} notificaciones", notificationRequests.Count);
          await _notificationService.SendBulkNotificationsAsync(notificationRequests);
          processed = notificationRequests.Count;
          _logger.LogDebug("Lote procesado exitosamente: {Count} notificaciones enviadas.", processed);
        }
        catch (HttpRequestException httpEx)
        {
          _logger.LogError(httpEx, "Error HTTP al enviar lote de notificaciones. Tamaño del lote: {BatchSize}, Mensaje: {Message}", notificationRequests.Count, httpEx.Message);
          errors = notificationRequests.Count;
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error general al enviar lote de notificaciones. Tamaño del lote: {BatchSize}", notificationRequests.Count);
          errors = notificationRequests.Count;
        }
      }
      else
      {
        _logger.LogDebug("Lote procesado: 0 notificaciones válidas para enviar (usuarios sin tokens push).");
      }

      return (processed, errors);
    }
  }
}