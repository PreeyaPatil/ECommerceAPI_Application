namespace ECommerceAPI.Entities
{
    public sealed class Customer : AuditableEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; }
        public bool IsMobileConfirmed { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public bool IsActive { get; set; } = true;

        public Cart? Cart { get; set; }
        public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();
    }
}
