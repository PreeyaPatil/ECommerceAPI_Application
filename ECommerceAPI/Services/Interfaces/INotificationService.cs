using ECommerceAPI.Entities;
namespace ECommerceAPI.Services.Interfaces
{
    public interface INotificationService
    {
        Task QueueEmailAsync(
            string recipient,
            string subject,
            string body);

        Task QueueSmsAsync(string recipient, string body);

        Task QueueEmailAndSmsAsync(
            string email,
            string phoneNumber,
            string subject,
            string emailBody,
            string smsBody);

        Task<List<NotificationQueueItem>> ClaimReadyForProcessingAsync();

        Task MarkAsSentAsync(NotificationQueueItem notification);

        Task MarkAsFailedAsync(NotificationQueueItem notification, string errorMessage);
    }
}
