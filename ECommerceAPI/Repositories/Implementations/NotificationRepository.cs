using ECommerceAPI.Data;
using ECommerceAPI.Entities;
using ECommerceAPI.Enums;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Repositories.Implementations
{
    public sealed class NotificationRepository : INotificationRepository
    {
        private readonly ECommerceDbContext _context;

        public NotificationRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        // Adds a single notification to the DbContext for insertion into the database.
        public async Task AddAsync(NotificationQueueItem notification)
        {
            await _context.NotificationQueueItems
                .AddAsync(notification);
        }

        // Adds multiple notifications to the DbContext in a single operation.
        public async Task AddRangeAsync(IEnumerable<NotificationQueueItem> notifications)
        {
            await _context.NotificationQueueItems
                .AddRangeAsync(notifications);
        }

        // Gets notifications that are ready and marks them as Processing
        public async Task<List<NotificationQueueItem>> ClaimReadyForProcessingAsync(
            int batchSize,                     // Maximum number of notifications to process in one batch.
            int maxRetries,                    // Maximum number of retry attempts allowed for a notification.
            DateTime utcNow,                   // Current UTC date and time used to check whether a notification is ready.
            DateTime staleProcessingBeforeUtc) // UTC time used to identify notifications that have been stuck in Processing for too long.
        {
            // Get notifications that have not exceeded the retry limit
            // and are ready for processing.
            var notifications = await _context.NotificationQueueItems
                .Where(x =>
                    x.RetryCount < maxRetries &&
                    (
                        // Pending notification whose processing time has arrived.
                        (x.NotificationStatusId == NotificationStatus.Pending &&
                         x.NextAttemptAtUtc <= utcNow)

                        ||

                        // Processing notification that has been stuck for too long.
                        (x.NotificationStatusId == NotificationStatus.Processing &&
                         x.ProcessingStartedAtUtc.HasValue &&
                         x.ProcessingStartedAtUtc <= staleProcessingBeforeUtc)
                    ))
                .OrderBy(x => x.NextAttemptAtUtc)
                .ThenBy(x => x.Id)
                .Take(batchSize)
                .ToListAsync();

            // Mark the selected notifications as Processing.
            foreach (var notification in notifications)
            {
                notification.NotificationStatusId = NotificationStatus.Processing;
                notification.ProcessingStartedAtUtc = utcNow;
                notification.ProcessedAtUtc = null;
                notification.UpdatedAtUtc = utcNow;
            }

            // Save the status changes to the database.
            await _context.SaveChangesAsync();

            // Return the notifications that are ready to be processed.
            return notifications;
        }
    }
}
