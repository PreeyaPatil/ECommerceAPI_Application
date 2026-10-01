using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [Authorize]
    [Route("api/orders")]
    public sealed class OrdersController : AuthenticatedControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // Place a new Order using the authenticated Customer's
        // Cart, selected Addresses, Payment Method,
        // and current server-generated CheckoutToken.
        // Endpoint: POST api/orders
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PlaceOrderResponseDTO>>> PlaceOrder(
                [FromBody] PlaceOrderRequestDTO request)
        {
            var customerId = GetCustomerId();

            _logger.LogInformation("Order Placement request received for Customer {CustomerId}.", customerId);

            var result = await _orderService.PlaceOrderAsync(customerId, request);

            _logger.LogInformation("Order Placement request completed successfully for Customer {CustomerId}.", customerId);

            var response = ApiResponse<PlaceOrderResponseDTO>
                    .SuccessResponse(
                        result,
                        "Order processed successfully.");

            return Ok(response);
        }

        // Retrieve paged Order History for the authenticated Customer.
        // Endpoint: GET api/orders
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<OrderListItemResponseDTO>>>> GetOrderHistory(
                [FromQuery] OrderHistoryRequestDTO request)
        {
            var customerId = GetCustomerId();

            _logger.LogInformation(
                "Retrieving Order History for Customer {CustomerId}. Page {PageNumber}, Page Size {PageSize}.",
                customerId,
                request.PageNumber,
                request.PageSize);

            var result = await _orderService.GetOrderHistoryAsync(customerId, request);

            _logger.LogInformation(
                "Order History retrieved successfully for Customer {CustomerId}.",
                customerId);

            var response = ApiResponse<PagedResult<OrderListItemResponseDTO>>
                    .SuccessResponse(
                        result,
                        "Order History retrieved successfully.");

            return Ok(response);
        }

        // Retrieve complete details of a specific Order belonging to the authenticated Customer.
        // Endpoint: GET api/orders/{orderId}
        [HttpGet("{orderId:int}")]
        public async Task<ActionResult<ApiResponse<OrderDetailsResponseDTO>>> GetOrderDetails(int orderId)
        {
            var customerId = GetCustomerId();

            _logger.LogInformation("Retrieving Order {OrderId} for Customer {CustomerId}.", orderId, customerId);

            var result = await _orderService.GetOrderDetailsAsync(customerId, orderId);

            _logger.LogInformation("Order {OrderId} retrieved successfully for Customer {CustomerId}.", orderId, customerId);

            var response = ApiResponse<OrderDetailsResponseDTO>
                    .SuccessResponse(
                        result,
                        "Order Details retrieved successfully.");

            return Ok(response);
        }
    }
}
