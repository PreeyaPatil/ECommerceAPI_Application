namespace ECommerceAPI.Options
{
    public sealed class NotificationOptions
    {
        public const string SectionName = "Notifications";
        public int BatchSize { get; set; } = 20;
        public int PollingSeconds { get; set; } = 60;
        public int MaxRetries { get; set; } = 3;
        public int ProcessingTimeoutMinutes { get; set; } = 5;
    }
}
