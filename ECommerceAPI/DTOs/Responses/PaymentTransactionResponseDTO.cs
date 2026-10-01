namespace ECommerceAPI.DTOs.Responses
{
    public sealed class PaymentTransactionResponseDTO
    {
        public int Id { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string? ProviderTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string? MaskedInstrument { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
