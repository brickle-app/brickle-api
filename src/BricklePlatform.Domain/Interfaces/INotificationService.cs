using BricklePlatform.Domain.DTOs;

namespace BricklePlatform.Domain.Interfaces
{
  public interface INotificationService
  {
    Task SendNotificationAsync(string recipientId, string title, string body, object? data = null);
    Task SendBulkNotificationsAsync(IEnumerable<NotificationDto> notifications);
  }
}