namespace ECommerceAPI.DTOs.Responses
{
    public sealed class PlaceOrderResponseDTO
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public string Currency { get; set; } = "INR";
        public string? FailureMessage { get; set; }
    }
}
