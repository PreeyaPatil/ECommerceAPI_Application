namespace ECommerceAPI.Entities
{
    public sealed class Category : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
