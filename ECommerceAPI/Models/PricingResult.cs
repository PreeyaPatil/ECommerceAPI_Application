namespace ECommerceAPI.Models
{
    public sealed class PricingResult
    {
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string Currency { get; set; } = "INR";
    }
}
