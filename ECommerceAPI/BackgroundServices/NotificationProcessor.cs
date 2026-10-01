using ECommerceAPI.Enums;
using ECommerceAPI.Options;
using ECommerceAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ECommerceAPI.BackgroundServices
{
    public sealed class NotificationProcessor : BackgroundService
    {
        // Used to create a new Dependency Injection scope.
        // This is required because BackgroundService is Singleton,
        // while application services such as INotificationService are Scoped.
        private readonly IServiceScopeFactory _scopeFactory;

        // Used to write Background Service logs.
        private readonly ILogger<NotificationProcessor> _logger;

        // Contains Notification processing settings such as
        // Polling interval, Batch Size, Retry limits, etc.
        private readonly NotificationOptions _notificationOptions;

        public NotificationProcessor(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationProcessor> logger,
            IOptions<NotificationOptions> notificationOptions)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            // Read NotificationOptions values from configuration.
            _notificationOptions = notificationOptions.Value;
        }

        // This method executes automatically when the application starts.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Processor started.");

            // Keep running until the application is shutting down.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Process one batch of ready Notifications.
                    await ProcessNotificationsAsync(stoppingToken);

                    // Wait before checking the Notification Queue again.
                    // Example:
                    // If PollingSeconds = 60,
                    // the processor checks the queue every 60 seconds.
                    await Task.Delay(TimeSpan.FromSeconds(_notificationOptions.PollingSeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    // Application shutdown was requested.
                    // Exit the processing loop.
                    break;
                }
                catch (Exception ex)
                {
                    // Log unexpected errors without stopping
                    // the Background Service permanently.
                    _logger.LogError(ex, "An error occurred while processing notifications.");
                }

                //try
                //{
                //    // Wait before checking the Notification Queue again.
                //    // Example:
                //    // If PollingSeconds = 10,
                //    // the processor checks the queue every 10 seconds.
                //    await Task.Delay(TimeSpan.FromSeconds(_notificationOptions.PollingSeconds), stoppingToken);
                //}
                //catch (OperationCanceledException)
                //    when (stoppingToken.IsCancellationRequested)
                //{
                //    // If the application stops while waiting, exit.
                //    break;
                //}
            }

            // This is logged when the application
            // is shutting down.
            _logger.LogInformation("Notification Processor stopped.");
        }

        private async Task ProcessNotificationsAsync(CancellationToken stoppingToken)
        {
            // BackgroundService is registered as Singleton.
            // INotificationService, IEmailService, and ISmsService
            // may depend on Scoped services such as DbContext.
            // Therefore, create a new DI scope for each processing cycle.
            await using var scope = _scopeFactory.CreateAsyncScope();

            // Resolve the Notification Service from the newly created scope.
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // Resolve the Email Service.
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Resolve the SMS Service.
            var smsService = scope.ServiceProvider.GetRequiredService<ISmsService>();

            // Claim a batch of Notifications that are ready for processing.
            // This can include:
            // 1. Pending Notifications whose NextAttemptAtUtc has arrived.
            // 2. Stale Processing Notifications that were stuck too long.
            var notifications = await notificationService.ClaimReadyForProcessingAsync();

            // Log how many Notifications were claimed during this processing cycle.
            _logger.LogInformation(
                "Claimed {NotificationCount} Notification(s) for processing.",
                notifications.Count);


            // Process each claimed Notification one by one.
            foreach (var notification in notifications)
            {
                // Stop processing new items if application shutdown has been requested.
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    // Decide how to send the Notification based on its configured Channel.
                    switch (notification.NotificationChannelId)
                    {
                        case NotificationChannel.Email:
                            _logger.LogInformation("Processing Email Notification {NotificationId}.", notification.Id);

                            // Send the Email using the Email Service.
                            await emailService
                                .SendEmailAsync(
                                    notification.Recipient,
                                    notification.Subject ?? "Notification",
                                    notification.Body);

                            break;

                        case NotificationChannel.Sms:
                            _logger.LogInformation("Processing SMS Notification {NotificationId}.", notification.Id);

                            // Send the SMS using the SMS Service.
                            await smsService
                                .SendSmsAsync(
                                    notification.Recipient,
                                    notification.Body);

                            break;

                        default:
                            // Reject unsupported Notification Channels.
                            throw new InvalidOperationException("Unsupported Notification Channel.");
                    }

                    // If Email/SMS delivery succeeds, mark the Notification as Sent.
                    await notificationService.MarkAsSentAsync(notification);

                    _logger.LogInformation(
                        "Notification {NotificationId} processed successfully.",
                        notification.Id);
                }
                catch (Exception ex)
                {
                    // Delivery failed for this specific Notification.
                    // Log the error, but continue processing
                    // the remaining Notifications.
                    _logger.LogWarning(ex, "Failed to process Notification {NotificationId}.", notification.Id);

                    try
                    {
                        // Update the Notification after failure.
                        // Depending on RetryCount:
                        // - it may return to Pending for another retry
                        // - or it may become permanently Failed
                        //   when MaxRetries has been reached.
                        await notificationService.MarkAsFailedAsync(notification, ex.Message);
                    }
                    catch (Exception updateException)
                    {
                        // Even updating the Notification status failed.
                        // Log this separately so the issue can be diagnosed.
                        _logger.LogError(
                            updateException,
                            "Failed to update Notification {NotificationId} after delivery failure.",
                            notification.Id);
                    }
                }
            }
        }
    }
}
