using BricklePlatform.Api.Application.Commands.Notifications;
using BricklePlatform.Api.Application.Models;
using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BricklePlatform.Api.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class NotificationsController : ControllerBase
  {
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(IMediator mediator, ILogger<NotificationsController> logger)
    {
      _mediator = mediator;
      _logger = logger;
    }

    /// <summary>
    /// Envía una notificación push a un solo dispositivo.
    /// </summary>
    /// <param name="command">Detalles de la notificación y el token del destinatario.</param>
    /// <returns>Resultado del envío.</returns>
    [HttpPost("Send")]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SendSingleNotification([FromBody] SendNotificationCommand command)
    {
      if (string.IsNullOrWhiteSpace(command.UserId))
      {
        ModelState.AddModelError(nameof(command.UserId), "El ID del usuario es requerido.");
      }
      if (string.IsNullOrWhiteSpace(command.ActionId))
      {
        ModelState.AddModelError(nameof(command.ActionId), "El ID de la acción es requerido.");
      }

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        var result = await _mediator.Send(command);
        return Ok(new { Success = result, Message = result ? "Notificación enviada exitosamente." : "Error al enviar la notificación." });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al enviar notificación individual. Token: {Token}", command.UserId);
        return StatusCode((int)HttpStatusCode.InternalServerError, "Ocurrió un error al enviar la notificación.");
      }
    }

    /// <summary>
    /// Envía una notificación push directamente a un token de Expo (para pruebas).
    /// </summary>
    /// <param name="request">Token de Expo y detalles de la notificación.</param>
    /// <returns>Resultado del envío.</returns>
    [HttpPost("SendDirect")]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SendDirectNotification([FromBody] SendDirectNotificationRequest request)
    {
      if (string.IsNullOrWhiteSpace(request.ExpoToken))
      {
        ModelState.AddModelError(nameof(request.ExpoToken), "El token de Expo es requerido.");
      }
      if (string.IsNullOrWhiteSpace(request.Title))
      {
        ModelState.AddModelError(nameof(request.Title), "El título es requerido.");
      }
      if (string.IsNullOrWhiteSpace(request.Body))
      {
        ModelState.AddModelError(nameof(request.Body), "El cuerpo es requerido.");
      }

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        _logger.LogInformation("Iniciando envío de notificación directa. Token: {Token}, Título: {Title}", request.ExpoToken, request.Title);
        
        var notificationService = HttpContext.RequestServices.GetRequiredService<BricklePlatform.Domain.Interfaces.INotificationService>();
        await notificationService.SendNotificationAsync(request.ExpoToken, request.Title, request.Body, request.Data);
        
        _logger.LogInformation("Notificación directa enviada exitosamente. Token: {Token}", request.ExpoToken);
        return Ok(new { Success = true, Message = "Notificación enviada exitosamente." });
      }
      catch (HttpRequestException httpEx)
      {
        _logger.LogError(httpEx, "Error HTTP al enviar notificación directa. Token: {Token}, Mensaje: {Message}", request.ExpoToken, httpEx.Message);
        return StatusCode((int)HttpStatusCode.BadGateway, new { Success = false, Message = $"Error al comunicar con Expo: {httpEx.Message}" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error general al enviar notificación directa. Token: {Token}", request.ExpoToken);
        return StatusCode((int)HttpStatusCode.InternalServerError, new { Success = false, Message = "Ocurrió un error al enviar la notificación.", Error = ex.Message });
      }
    }

    /// <summary>
    /// Envía la misma notificación push a TODOS los usuarios registrados en la plataforma.
    /// Esta operación puede ser lenta y consumir muchos recursos. Úsala con cuidado.
    /// </summary>
    /// <param name="command">Detalles de la notificación a enviar masivamente.</param>
    /// <returns>Resultado del inicio del proceso de envío.</returns>
    [HttpPost("SendBulk")]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    // [Authorize(Roles = "Admin")] // Ejemplo: Solo accesible por administradores
    public async Task<IActionResult> SendBulkNotification([FromBody] SendBulkNotificationCommand command)
    {
      if (string.IsNullOrWhiteSpace(command.Title))
      {
        ModelState.AddModelError(nameof(command.Title), "El título es requerido.");
      }
      if (string.IsNullOrWhiteSpace(command.Body))
      {
        ModelState.AddModelError(nameof(command.Body), "El cuerpo es requerido.");
      }

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        var result = await _mediator.Send(command);

        if (result)
        {
          return Ok(new { Success = true, Message = "Proceso de envío masivo iniciado exitosamente." });
        }
        else
        {
          return StatusCode((int)HttpStatusCode.InternalServerError, new { Success = false, Message = "No se pudo iniciar el proceso de envío masivo." });
        }

      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al iniciar el envío masivo de notificaciones. Título: {Title}", command.Title);
        return StatusCode((int)HttpStatusCode.InternalServerError, "Ocurrió un error al iniciar el envío masivo de notificaciones.");
      }
    }

    /// <summary>
    /// Tras un envío on-chain entre usuarios Brickle, notifica al destinatario por push (Expo).
    /// El remitente se identifica por el header <c>user</c> (email), igual que en el resto de la API móvil.
    /// </summary>
    [HttpPost("NotifyIncomingTransfer")]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> NotifyIncomingTransfer(
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] NotifyIncomingTransferRequest body)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if (string.IsNullOrWhiteSpace(body.RecipientWalletAddress))
      {
        ModelState.AddModelError(nameof(body.RecipientWalletAddress), "Requerido.");
        return BadRequest(ModelState);
      }

      if (string.IsNullOrWhiteSpace(body.Amount))
      {
        ModelState.AddModelError(nameof(body.Amount), "Requerido.");
        return BadRequest(ModelState);
      }

      try
      {
        var result = await _mediator.Send(new NotifyIncomingTransferCommand(
            header.User,
            body.RecipientWalletAddress.Trim(),
            body.Amount.Trim(),
            string.IsNullOrWhiteSpace(body.TransactionHash)
                ? null
                : body.TransactionHash.Trim()));

        return Ok(new
        {
          Success = result.Notified,
          Message = result.Message
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "NotifyIncomingTransfer falló para remitente {Email}", header.User);
        return StatusCode(
            (int)HttpStatusCode.InternalServerError,
            new { Success = false, Message = "Error al procesar la solicitud." });
      }
    }
  }
}