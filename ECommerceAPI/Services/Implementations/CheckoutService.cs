using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Helpers;
using ECommerceAPI.Mappings;
using ECommerceAPI.Options;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ECommerceAPI.Services.Implementations
{
    public sealed class CheckoutService : ICheckoutService
    {
        // Used to retrieve the Customer's Shopping Cart.
        private readonly ICartRepository _cartRepository;

        // Used to retrieve the Customer's saved Addresses.
        private readonly ICustomerAddressRepository _addressRepository;

        // Used to retrieve the active Payment Methods.
        private readonly IPaymentRepository _paymentRepository;

        // Contains pricing-related configuration such as
        // Tax Rate, Shipping Charge, Free Shipping Threshold, and Currency.
        private readonly PricingOptions _pricingOptions;

        // Used to write application logs.
        private readonly ILogger<CheckoutService> _logger;

        public CheckoutService(
            ICartRepository cartRepository,
            ICustomerAddressRepository addressRepository,
            IPaymentRepository paymentRepository,
            IOptions<PricingOptions> pricingOptions,
            ILogger<CheckoutService> logger)
        {
            _cartRepository = cartRepository;
            _addressRepository = addressRepository;
            _paymentRepository = paymentRepository;

            // Read PricingOptions values from configuration.
            _pricingOptions = pricingOptions.Value;

            _logger = logger;
        }

        public async Task<CheckoutResponseDTO> GetCheckoutAsync(int customerId)
        {
            _logger.LogInformation("Preparing Checkout information for Customer {CustomerId}.", customerId);

            // Retrieve the Customer's Cart including Cart Items
            // and related Product information.
            var cart = await _cartRepository.GetByCustomerIdAsync(customerId);

            // Checkout cannot continue if the Cart does not exist or does not contain any Items.
            if (cart == null || cart.Items.Count == 0)
            {
                _logger.LogWarning("Checkout failed because Customer {CustomerId} Shopping Cart is empty.", customerId);

                throw new BusinessException("Shopping Cart is empty.");
            }

            // Validate every Product currently available in the Cart.
            foreach (var item in cart.Items)
            {
                // Product must still be active.
                if (!item.Product.IsActive)
                {
                    _logger.LogWarning(
                        "Checkout failed because Product {ProductId} is no longer available.",
                        item.ProductId);

                    throw new BusinessException($"{item.Product.Name} is no longer available.");
                }

                // Cart quantity must not exceed the latest available stock.
                if (item.Quantity > item.Product.StockQuantity)
                {
                    throw new BusinessException(
                        $"Only {item.Product.StockQuantity} " +
                        $"unit(s) of {item.Product.Name} are available.");
                }
            }

            // Calculate the Cart Subtotal using the latest Product Prices.
            var subtotal = cart.Items.Sum(x => x.Product.Price * x.Quantity);

            // Calculate Tax, Shipping, Grand Total, and Currency
            // using the configured PricingOptions.
            var pricingResult = PricingHelper.Calculate(subtotal, _pricingOptions);

            // Convert the calculated pricing values
            // into the Response DTO used by the Checkout page.
            var pricing = new PricingSummaryResponseDTO
            {
                Subtotal = pricingResult.Subtotal,
                TaxAmount = pricingResult.TaxAmount,
                ShippingAmount = pricingResult.ShippingAmount,
                GrandTotal = pricingResult.GrandTotal,
                Currency = pricingResult.Currency
            };

            // Retrieve all saved Customer Addresses
            // so the Customer can select Shipping and Billing Addresses.
            var addresses = await _addressRepository.GetByCustomerIdAsync(customerId);

            // Retrieve all currently active Payment Methods
            // such as Cash on Delivery, Card, and UPI.
            var paymentMethods = await _paymentRepository.GetActivePaymentMethodsAsync();

            _logger.LogInformation("Checkout information prepared successfully for Customer {CustomerId}.", customerId);

            // Combine Cart Items, Pricing, Addresses,
            // and Payment Methods into CheckoutResponseDTO.
            return cart.ToCheckoutResponseDTO(pricing, addresses, paymentMethods);
        }
    }
}
