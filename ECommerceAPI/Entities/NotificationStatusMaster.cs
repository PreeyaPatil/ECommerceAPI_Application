using ECommerceAPI.Enums;
namespace ECommerceAPI.Entities
{
    public sealed class NotificationStatusMaster : AuditableEntity
    {
        public NotificationStatus Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}
