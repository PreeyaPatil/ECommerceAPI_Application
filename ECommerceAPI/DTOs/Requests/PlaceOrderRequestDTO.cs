using ECommerceAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs.Requests
{
    public sealed class PlaceOrderRequestDTO
    {
        public Guid CheckoutToken { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Shipping Address Id must be greater than 0.")]
        public int ShippingAddressId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Billing Address Id must be greater than 0.")]
        public int BillingAddressId { get; set; }

        [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Please select a valid Payment Method.")]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
