using ECommerceAPI.Enums;
namespace ECommerceAPI.Entities
{
    public sealed class VerificationCode : AuditableEntity
    {
        public long Id { get; set; }
        public int CustomerId { get; set; }
        public VerificationPurpose Purpose { get; set; } //2FA, Email Verification, Mobile Verification
        public string Identifier { get; set; } = string.Empty; //Email, Mobile
        public string CodeHash { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime? UsedAtUtc { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public Customer Customer { get; set; } = null!;
    }
}
