namespace BricklePlatform.Api.Application.Models;

public class SendDirectNotificationRequest
{
    public string ExpoToken { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public object? Data { get; set; }
}