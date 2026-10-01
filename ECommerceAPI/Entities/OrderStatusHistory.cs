using ECommerceAPI.Enums;
namespace ECommerceAPI.Entities
{
    public sealed class OrderStatusHistory : AuditableEntity
    {
        public long Id { get; set; }
        public int OrderId { get; set; }
        public OrderStatus? PreviousStatusId { get; set; }
        public OrderStatus NewStatusId { get; set; }
        public string? Remarks { get; set; }
        public Order Order { get; set; } = null!;
        public OrderStatusMaster? PreviousStatus { get; set; }
        public OrderStatusMaster NewStatus { get; set; } = null!;
    }
}

