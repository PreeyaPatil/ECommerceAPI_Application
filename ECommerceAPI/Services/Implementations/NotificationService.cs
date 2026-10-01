using ECommerceAPI.Entities;
using ECommerceAPI.Enums;
using ECommerceAPI.Options;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ECommerceAPI.Services.Implementations
{
    public sealed class NotificationService : INotificationService
    {
        // Used to add, claim/get, and update Notification Queue records.
        private readonly INotificationRepository _notificationRepository;

        // Used to save database changes.
        private readonly IUnitOfWork _unitOfWork;

        // Contains Notification processing settings such as
        // Batch Size, Max Retries, and Processing Timeout.
        private readonly NotificationOptions _notificationOptions;

        // Used to write application logs.
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork,
            IOptions<NotificationOptions> notificationOptions,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;

            // Read NotificationOptions values from configuration.
            _notificationOptions = notificationOptions.Value;

            _logger = logger;
        }

        public async Task QueueEmailAsync(string recipient, string subject, string body)
        {
            // Create an Email Notification Queue item.
            var notification = CreateNotification(NotificationChannel.Email, recipient, subject, body);

            // Add the Notification to the database.
            await _notificationRepository.AddAsync(notification);

            // Save the queued Notification.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Email Notification {NotificationId} added to the Notification Queue.",
                notification.Id);
        }

        public async Task QueueSmsAsync(string recipient, string body)
        {
            // Create an SMS Notification Queue item.
            // SMS does not require a Subject.
            var notification = CreateNotification(NotificationChannel.Sms, recipient, null, body);

            // Add the Notification to the database.
            await _notificationRepository.AddAsync(notification);

            // Save the queued Notification.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "SMS Notification {NotificationId} added to the Notification Queue.",
                notification.Id);
        }

        public async Task QueueEmailAndSmsAsync(
            string email,
            string phoneNumber,
            string subject,
            string emailBody,
            string smsBody)
        {
            // Create both Email and SMS Notifications together.
            var notifications = new List<NotificationQueueItem>
            {
                CreateNotification(NotificationChannel.Email, email, subject, emailBody),
                CreateNotification(NotificationChannel.Sms, phoneNumber, null, smsBody)
            };

            // Add both Notifications to the queue in one operation.
            await _notificationRepository.AddRangeAsync(notifications);

            // Save both queued Notifications.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Email and SMS Notifications added to the Notification Queue.");
        }

        public Task<List<NotificationQueueItem>> ClaimReadyForProcessingAsync()
        {
            _logger.LogInformation(
                "Claiming pending Notifications for processing. Batch Size: {BatchSize}.",
                _notificationOptions.BatchSize);

            // Get the current UTC time.
            // This is mainly used to select Pending Notifications
            // whose NextAttemptAtUtc time has already arrived.

            // Example:
            // utcNow = 10:00 AM

            // Notification A:
            // Status = Pending
            // NextAttemptAtUtc = 9:55 AM
            // => Ready for processing because 9:55 AM <= 10:00 AM

            // Notification B:
            // Status = Pending
            // NextAttemptAtUtc = 10:10 AM
            // => Not ready yet because 10:10 AM > 10:00 AM
            var utcNow = DateTime.UtcNow;

            // Calculate the time before which a Processing Notification
            // is considered stale or stuck.

            // Example:
            // utcNow = 10:00 AM
            // ProcessingTimeoutMinutes = 5
            // staleProcessingBeforeUtc = 9:55 AM

            // Notification C:
            // Status = Processing
            // ProcessingStartedAtUtc = 9:50 AM
            // => Considered stale because 9:50 AM <= 9:55 AM

            // Notification D:
            // Status = Processing
            // ProcessingStartedAtUtc = 9:58 AM
            // => Not stale because it has only been processing for 2 minutes
            var staleProcessingBeforeUtc =
                utcNow.AddMinutes(-_notificationOptions.ProcessingTimeoutMinutes);

            // Claim a batch of Notifications that are ready
            // to be processed by the Background Service.

            // The Repository will normally mark those claimed records
            // as Processing so that another Background Service instance
            // does not process the same records at the same time.
            return _notificationRepository
                .ClaimReadyForProcessingAsync(
                    _notificationOptions.BatchSize,
                    _notificationOptions.MaxRetries,
                    utcNow,
                    staleProcessingBeforeUtc);
        }

        public async Task MarkAsSentAsync(NotificationQueueItem notification)
        {
            var utcNow = DateTime.UtcNow;

            // Mark the Notification as successfully sent.
            notification.NotificationStatusId = NotificationStatus.Sent;

            // Record when processing completed.
            notification.ProcessedAtUtc = utcNow;

            // Clear ProcessingStartedAtUtc because processing has finished.
            notification.ProcessingStartedAtUtc = null;

            // Clear any previous error message.
            notification.LastError = null;

            // Record when the Notification was updated.
            notification.UpdatedAtUtc = utcNow;

            // Save the updated Notification status.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Notification {NotificationId} marked as Sent.", notification.Id);
        }

        public async Task MarkAsFailedAsync(NotificationQueueItem notification, string errorMessage)
        {
            var utcNow = DateTime.UtcNow;

            // Increase the retry count because the current processing attempt failed.
            notification.RetryCount++;

            // Clear the Processing Started time because this attempt has completed.
            notification.ProcessingStartedAtUtc = null;

            notification.UpdatedAtUtc = utcNow;

            // Store the error message, but limit its length
            // so an excessively large error is not stored.
            notification.LastError =
                errorMessage.Length > 2000
                    ? errorMessage[..2000]
                    : errorMessage;

            // If the maximum retry count has been reached,
            // permanently mark the Notification as Failed.
            if (notification.RetryCount >= _notificationOptions.MaxRetries)
            {
                notification.NotificationStatusId = NotificationStatus.Failed;
                notification.ProcessedAtUtc = utcNow;
            }
            else
            {
                // Otherwise, move the Notification back
                // to Pending so it can be retried later.
                notification.NotificationStatusId = NotificationStatus.Pending;

                notification.ProcessedAtUtc = null;

                // Calculate exponential retry delay:
                // Retry 1 -> 2 minutes
                // Retry 2 -> 4 minutes
                // Retry 3 -> 8 minutes
                // and so on, with a maximum delay of 30 minutes.
                var retryDelayMinutes = Math.Min(Math.Pow(2, notification.RetryCount), 30);

                // Schedule the next processing attempt.
                notification.NextAttemptAtUtc = utcNow.AddMinutes(retryDelayMinutes);
            }

            // Save the updated Notification state.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogWarning(
                "Notification {NotificationId} processing failed. Retry Count: {RetryCount}, Status: {Status}.",
                notification.Id,
                notification.RetryCount,
                notification.NotificationStatusId);
        }

        private static NotificationQueueItem CreateNotification(
            NotificationChannel channel, 
            string recipient, 
            string? subject, 
            string body)
        {
            // Create a new Notification Queue record.
            // Every new Notification starts with Pending status
            // and is ready for its first processing attempt.
            return new NotificationQueueItem
            {
                NotificationChannelId = channel,
                NotificationStatusId = NotificationStatus.Pending,
                Recipient = recipient,
                Subject = subject,
                Body = body,

                // No retry has happened yet.
                RetryCount = 0,

                // Make the Notification immediately available for Background Service processing.
                NextAttemptAtUtc = DateTime.UtcNow
            };
        }
    }
}
