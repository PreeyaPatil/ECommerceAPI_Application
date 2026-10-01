namespace ECommerceAPI.Entities
{
    public sealed class Cart : AuditableEntity
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Guid CheckoutToken { get; set; } = Guid.NewGuid();
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public Customer Customer { get; set; } = null!;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
