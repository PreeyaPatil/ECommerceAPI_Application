using ECommerceAPI.Enums;
namespace ECommerceAPI.Entities
{
    public sealed class NotificationChannelMaster : AuditableEntity
    {
        public NotificationChannel Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}

