using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;

namespace ECommerceAPI.Mappings
{

    public static class CheckoutMappingExtensions
    {
        // Converts checkout-related data into a single CheckoutResponseDTO.
        public static CheckoutResponseDTO ToCheckoutResponseDTO(
            this Cart cart,
            PricingSummaryResponseDTO pricing,
            IEnumerable<CustomerAddress> addresses,
            IEnumerable<PaymentMethodMaster> paymentMethods)
        {
            return new CheckoutResponseDTO
            {
                // Include the checkout token associated with the current cart.
                CheckoutToken = cart.CheckoutToken,

                // Convert all cart items into checkout item response DTOs.
                Items = cart.Items
                    .Select(item => item.ToCheckoutItemResponseDTO())
                    .ToList(),

                // Include the calculated pricing summary.
                Pricing = pricing,

                // Convert the customer's saved addresses into response DTOs.
                Addresses = addresses
                    .Select(address => address.ToResponseDTO())
                    .ToList(),

                // Convert the available payment methods into response DTOs.
                PaymentMethods = paymentMethods
                    .Select(paymentMethod => paymentMethod.ToResponseDTO())
                    .ToList()
            };
        }
    }
}
