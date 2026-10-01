namespace ECommerceAPI.Options
{
    public sealed class TwoFactorOptions
    {
        public const string SectionName = "TwoFactor";
        public int CodeExpiryMinutes { get; set; } = 5;
        public int MaxFailedAttempts { get; set; } = 5;
    }
}
