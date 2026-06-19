using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using BricklePlatform.Domain.DTOs;


namespace BricklePlatform.Infrastructure.Services
{
  public class ExpoNotificationService : INotificationService
  {
    private readonly HttpClient _httpClient;
    private readonly ExpoSettings _expoSettings;
    private readonly ILogger<ExpoNotificationService> _logger;

    public ExpoNotificationService(HttpClient httpClient, IOptions<ExpoSettings> expoSettings, ILogger<ExpoNotificationService> logger)
    {
      _httpClient = httpClient;
      _expoSettings = expoSettings.Value;
      _logger = logger;
    }

    public async Task SendNotificationAsync(string recipientId, string title, string body, object? data = null)
    {
      // Validar formato del token
      if (string.IsNullOrEmpty(recipientId) || !IsValidExpoToken(recipientId))
      {
        throw new ArgumentException($"Token de Expo inválido: {recipientId}");
      }

      // Asegurar que data sea un objeto, no un string u otro tipo primitivo
      var dataObject = ConvertToDataObject(data);

      var message = new
      {
        to = recipientId,
        title = title,
        body = body,
        data = dataObject
      };

      var messages = new List<object> { message };
      await SendNotificationsInternalAsync(messages);
    }

    public async Task SendBulkNotificationsAsync(IEnumerable<NotificationDto> notifications)
    {
      var messages = notifications.Select(notification => new
      {
        to = notification.RecipientId,
        title = notification.Title,
        body = notification.Body,
        data = ConvertToDataObject(notification.Data)
      }).ToList();

      await SendNotificationsInternalAsync(messages);
    }

    private async Task SendNotificationsInternalAsync(IEnumerable<object> messages)
    {
      try
      {
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var json = JsonSerializer.Serialize(messages, new JsonSerializerOptions 
        { 
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
          WriteIndented = true 
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Enviando {Count} notificaciones a Expo API: {Endpoint}", messages.Count(), _expoSettings.PushEndpoint);
        _logger.LogDebug("Payload de notificaciones: {Payload}", json);

        var httpResponse = await _httpClient.PostAsync(_expoSettings.PushEndpoint, content);

        var responseContent = await httpResponse.Content.ReadAsStringAsync();

        if (httpResponse.IsSuccessStatusCode)
        {
          _logger.LogInformation("Respuesta exitosa de Expo API: {Response}", responseContent);
          
          // Verificar si hay errores en la respuesta de Expo
          try
          {
            var expoResponse = JsonSerializer.Deserialize<ExpoResponse>(responseContent);
            if (expoResponse?.data?.Any(d => d.status == "error") == true)
            {
              var errors = expoResponse.data.Where(d => d.status == "error").ToList();
              _logger.LogWarning("Expo API reportó {ErrorCount} errores en {TotalCount} notificaciones: {Errors}", 
                errors.Count, expoResponse.data.Count, JsonSerializer.Serialize(errors));
            }
          }
          catch (JsonException jsonEx)
          {
            _logger.LogWarning(jsonEx, "No se pudo parsear la respuesta de Expo API, pero el HTTP status fue exitoso");
          }
        }
        else
        {
          _logger.LogError("Error al enviar notificación a Expo API. Código: {StatusCode}, Mensaje: {ErrorMessage}", httpResponse.StatusCode, responseContent);
          throw new HttpRequestException($"Error al enviar notificación: {httpResponse.StatusCode}, {responseContent}");
        }
      }
      catch (Exception ex) when (ex is not HttpRequestException)
      {
        _logger.LogError(ex, "Excepción al enviar notificaciones a Expo API");
        throw new HttpRequestException($"Error al enviar notificación: {ex.Message}", ex);
      }
    }

    private static bool IsValidExpoToken(string token)
    {
      // Validar que el token tenga el formato correcto de Expo
      return !string.IsNullOrEmpty(token) && 
             (token.StartsWith("ExponentPushToken[") || 
              token.StartsWith("ExpoPushToken[") ||
              // Formato alternativo para tokens de desarrollo
              token.Length > 20);
    }

    private static Dictionary<string, object> ConvertToDataObject(object? data)
    {
      if (data == null)
        return new Dictionary<string, object>();

      // Si ya es un Dictionary, devolverlo directamente
      if (data is Dictionary<string, object> dict)
        return dict;

      // Si es un string, crear un objeto con una propiedad "message"
      if (data is string str)
        return new Dictionary<string, object> { { "message", str } };

      // Para otros tipos de objetos, intentar convertir usando reflexión
      if (data.GetType().IsClass && data.GetType() != typeof(string))
      {
        var result = new Dictionary<string, object>();
        var properties = data.GetType().GetProperties();
        
        foreach (var prop in properties)
        {
          var value = prop.GetValue(data);
          if (value != null)
          {
            result[prop.Name] = value;
          }
        }
        return result;
      }

      // Para tipos primitivos, crear un objeto con una propiedad "value"
      return new Dictionary<string, object> { { "value", data } };
    }

  }

  // Models para parsear la respuesta de Expo API
  public class ExpoResponse
  {
    public List<ExpoReceiptData> data { get; set; } = new();
  }

  public class ExpoReceiptData
  {
    public string status { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
    public ExpoErrorDetails details { get; set; } = new();
  }

  public class ExpoErrorDetails
  {
    public string error { get; set; } = string.Empty;
  }
}