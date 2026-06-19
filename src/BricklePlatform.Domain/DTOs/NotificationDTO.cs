namespace BricklePlatform.Domain.DTOs;

public class NotificationDto
{
  public string RecipientId { get; set; }
  public string Title { get; set; }
  public string Body { get; set; }
  public object? Data { get; set; }
}