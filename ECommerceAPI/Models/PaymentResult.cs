using ECommerceAPI.Enums;
namespace ECommerceAPI.Models
{
    public sealed class PaymentResult
    {
        public PaymentStatus Status { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string? ProviderTransactionId { get; set; }
        public string? MaskedInstrument { get; set; }
        public string? FailureReason { get; set; }
    }
}
