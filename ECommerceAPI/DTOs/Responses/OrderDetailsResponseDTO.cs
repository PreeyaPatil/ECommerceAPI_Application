namespace ECommerceAPI.DTOs.Responses
{
    public sealed class OrderDetailsResponseDTO
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDateUtc { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public PricingSummaryResponseDTO Pricing { get; set; } = null!;
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;

        public List<OrderItemResponseDTO> Items { get; set; }
            = new List<OrderItemResponseDTO>();

        public List<PaymentTransactionResponseDTO> Payments { get; set; }
            = new List<PaymentTransactionResponseDTO>();

        public List<OrderStatusHistoryResponseDTO> StatusHistory { get; set; }
            = new List<OrderStatusHistoryResponseDTO>();
    }
}
