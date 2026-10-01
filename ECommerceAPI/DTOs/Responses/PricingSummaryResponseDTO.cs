namespace ECommerceAPI.DTOs.Responses
{
    public sealed class PricingSummaryResponseDTO
    {
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string Currency { get; set; } = "INR";
    }
}
