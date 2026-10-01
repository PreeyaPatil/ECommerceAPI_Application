using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [Authorize]
    [Route("api/cart")]
    public sealed class CartController : AuthenticatedControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        // Retrieve the complete Shopping Cart
        // of the authenticated Customer.
        // Endpoint: GET api/cart
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartResponseDTO>>> GetCart()
        {
            var customerId = GetCustomerId();

            _logger.LogInformation("Retrieving Shopping Cart for Customer {CustomerId}.", customerId);

            var result = await _cartService.GetCartAsync(customerId);

            _logger.LogInformation("Shopping Cart retrieved successfully for Customer {CustomerId}.", customerId);

            var response = ApiResponse<CartResponseDTO>
                    .SuccessResponse(
                        result,
                        "Shopping Cart retrieved successfully.");

            return Ok(response);
        }

        // Add a Product to the authenticated Customer's Cart
        // or increase the quantity when it already exists.
        // Endpoint: POST api/cart/items
        [HttpPost("items")]
        public async Task<ActionResult<ApiResponse<CartResponseDTO>>> AddToCart(
            [FromBody] AddToCartRequestDTO request)
        {
            var customerId = GetCustomerId();

            _logger.LogInformation(
                "Add to Cart request received for Customer {CustomerId}, Product {ProductId}.",
                customerId,
                request.ProductId);

            var result = await _cartService.AddToCartAsync(customerId, request);

            _logger.LogInformation(
                "Product {ProductId} added or updated successfully in Customer {CustomerId} Cart.",
                request.ProductId,
                customerId);

            var response = ApiResponse<CartResponseDTO>
                    .SuccessResponse(
                        result,
                        "Product added to Shopping Cart successfully.");

            return Ok(response);
        }

        // Update the quantity of an existing Cart Item
        // belonging to the authenticated Customer.
        // Endpoint: PUT api/cart/items/{cartItemId}
        [HttpPut("items/{cartItemId:int}")]
        public async Task<ActionResult<ApiResponse<CartResponseDTO>>> UpdateQuantity(
            int cartItemId,
            [FromBody] UpdateCartItemQuantityRequestDTO request)
        {
            var customerId = GetCustomerId();

            _logger.LogInformation(
                "Updating Cart Item {CartItemId} for Customer {CustomerId}.",
                cartItemId,
                customerId);

            var result = await _cartService.UpdateQuantityAsync(customerId, cartItemId, request);

            _logger.LogInformation(
                "Cart Item {CartItemId} updated successfully for Customer {CustomerId}.",
                cartItemId,
                customerId);

            var response = ApiResponse<CartResponseDTO>
                    .SuccessResponse(
                        result,
                        "Cart Item quantity updated successfully.");

            return Ok(response);
        }

        // Remove a specific Cart Item
        // from the authenticated Customer's Cart.
        // Endpoint: DELETE api/cart/items/{cartItemId}
        [HttpDelete("items/{cartItemId:int}")]
        public async Task<ActionResult<ApiResponse<string>>> RemoveItem(int cartItemId)
        {
            var customerId = GetCustomerId();

            _logger.LogInformation(
                "Removing Cart Item {CartItemId} for Customer {CustomerId}.",
                cartItemId,
                customerId);

            await _cartService.RemoveItemAsync(customerId, cartItemId);

            _logger.LogInformation(
                "Cart Item {CartItemId} removed successfully for Customer {CustomerId}.",
                cartItemId,
                customerId);

            var response = ApiResponse<string>.SuccessResponse(
                    "Cart Item removed.",
                    "Cart Item removed successfully.");

            return Ok(response);
        }

        // Remove all Items from
        // the authenticated Customer's Shopping Cart.
        // Endpoint: DELETE api/cart/items
        [HttpDelete("items")]
        public async Task<ActionResult<ApiResponse<string>>> ClearCart()
        {
            var customerId = GetCustomerId();

            _logger.LogInformation("Clear Shopping Cart request received for Customer {CustomerId}.", customerId);

            await _cartService.ClearCartAsync(customerId);

            _logger.LogInformation("Shopping Cart cleared successfully for Customer {CustomerId}.", customerId);

            var response = ApiResponse<string>.SuccessResponse(
                    "Shopping Cart cleared.",
                    "Shopping Cart cleared successfully.");

            return Ok(response);
        }
    }
}
