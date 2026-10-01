namespace ECommerceAPI.DTOs.Responses
{
    public sealed class CheckoutResponseDTO
    {
        public Guid CheckoutToken { get; set; }

        public List<CheckoutItemResponseDTO> Items { get; set; }
            = new List<CheckoutItemResponseDTO>();

        public PricingSummaryResponseDTO Pricing { get; set; } = null!;

        public List<CustomerAddressResponseDTO> Addresses { get; set; }
            = new List<CustomerAddressResponseDTO>();

        public List<PaymentMethodResponseDTO> PaymentMethods { get; set; }
            = new List<PaymentMethodResponseDTO>();
    }
}
