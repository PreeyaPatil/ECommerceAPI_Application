using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [Authorize]
    [Route("api/checkout")]
    public sealed class CheckoutController : AuthenticatedControllerBase
    {
        private readonly ICheckoutService _checkoutService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ICheckoutService checkoutService,
            ILogger<CheckoutController> logger)
        {
            _checkoutService = checkoutService;
            _logger = logger;
        }

        // Prepare the latest Checkout information,
        // including Cart Items, Pricing, Addresses,
        // Payment Methods, and CheckoutToken.
        // Endpoint: GET api/checkout
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CheckoutResponseDTO>>> GetCheckout()
        {
            var customerId = GetCustomerId();

            _logger.LogInformation("Preparing Checkout information for Customer {CustomerId}.", customerId);

            var result = await _checkoutService.GetCheckoutAsync(customerId);

            _logger.LogInformation("Checkout information prepared successfully for Customer {CustomerId}.", customerId);

            var response = ApiResponse<CheckoutResponseDTO>
                    .SuccessResponse(
                        result,
                        "Checkout information prepared successfully.");

            return Ok(response);
        }
    }
}
