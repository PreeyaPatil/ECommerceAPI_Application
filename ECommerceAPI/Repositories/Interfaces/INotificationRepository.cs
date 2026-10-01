using ECommerceAPI.Entities;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        // Adds a single notification to the notification queue.
        Task AddAsync(NotificationQueueItem notification);

        // Adds multiple notifications to the notification queue at once.
        Task AddRangeAsync(IEnumerable<NotificationQueueItem> notifications);

        // Finds and safely claims notifications that are ready to be processed.
        Task<List<NotificationQueueItem>> ClaimReadyForProcessingAsync(
            int batchSize,
            int maxRetries,
            DateTime utcNow, 
            DateTime staleProcessingBeforeUtc);
    }
}
