namespace ECommerceAPI.Options
{
    public sealed class VerificationOptions
    {
        public const string SectionName = "Verification";
        public int CodeExpiryMinutes { get; set; } = 10;
        public int MaxFailedAttempts { get; set; } = 5;
    }
}
