namespace ECommerceAPI.Options
{
    public sealed class PricingOptions
    {
        public const string SectionName = "Pricing";
        public decimal TaxRate { get; set; } = 0.18m;
        public decimal FlatShippingCharge { get; set; } = 99m;
        public decimal FreeShippingThreshold { get; set; } = 2000m;
    }
}
