namespace ECommerceAPI.DTOs.Responses
{
    public sealed class PaymentMethodResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool RequiresOnlinePayment { get; set; }
    }
}
