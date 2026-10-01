using ECommerceAPI.Enums;
namespace ECommerceAPI.Entities
{
    public sealed class Order : AuditableEntity
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public Guid CheckoutToken { get; set; }
        public PaymentMethod PaymentMethodId { get; set; }
        public PaymentStatus PaymentStatusId { get; set; } = ECommerceAPI.Enums.PaymentStatus.Pending;
        public OrderStatus OrderStatusId { get; set; } = ECommerceAPI.Enums.OrderStatus.Pending;
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string Currency { get; set; } = "INR";
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public Customer Customer { get; set; } = null!;
        public PaymentMethodMaster PaymentMethod { get; set; } = null!;
        public PaymentStatusMaster PaymentStatus { get; set; } = null!;
        public OrderStatusMaster OrderStatus { get; set; } = null!;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<PaymentTransaction> Payments { get; set; } = new List<PaymentTransaction>();
        public ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
    }
}
