namespace ECommerceAPI.Models
{
    public sealed class RefreshTokenResult
    {
        public string PlainTextToken { get; set; } = string.Empty;
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
