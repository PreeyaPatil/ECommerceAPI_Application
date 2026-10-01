namespace ECommerceAPI.DTOs.Responses
{
    public sealed class OrderStatusHistoryResponseDTO
    {
        public long Id { get; set; }
        public string? PreviousStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime ChangedAtUtc { get; set; }
    }
}
