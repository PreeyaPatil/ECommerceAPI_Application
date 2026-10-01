namespace ECommerceAPI.Entities
{
    public sealed class RefreshToken : AuditableEntity
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public Customer Customer { get; set; } = null!;
    }
}
