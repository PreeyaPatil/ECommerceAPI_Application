using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [Authorize]
    [Route("api/addresses")]
    public sealed class CustomerAddressesController : AuthenticatedControllerBase
    {
        private readonly ICustomerAddressService _addressService;
        private readonly ILogger<CustomerAddressesController> _logger;

        public CustomerAddressesController(
            ICustomerAddressService addressService,
            ILogger<CustomerAddressesController> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }

        // Retrieve all saved Addresses belonging to the authenticated Customer.
        // Endpoint: GET api/addresses
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CustomerAddressResponseDTO>>>> GetAddresses()
        {
            var customerId = GetCustomerId();

            _logger.LogInformation("Retrieving Addresses for Customer {CustomerId}.", customerId);

            var result = await _addressService.GetAddressesAsync(customerId);

            _logger.LogInformation("Addresses retrieved successfully for Customer {CustomerId}.", customerId);

            var response = ApiResponse<List<CustomerAddressResponseDTO>>
                    .SuccessResponse(
                        result,
                        "Addresses retrieved successfully.");

            return Ok(response);
        }

        // Retrieve a specific Address belonging to the authenticated Customer.
        // Endpoint: GET api/addresses/{addressId}
        [HttpGet("{addressId:int}")]
        public async Task<ActionResult<ApiResponse<CustomerAddressResponseDTO>>> GetAddress(int addressId)
        {
            var customerId = GetCustomerId();

            _logger.LogInformation("Retrieving Address {AddressId} for Customer {CustomerId}.", addressId, customerId);

            var result = await _addressService.GetAddressAsync(customerId, addressId);

            _logger.LogInformation("Address {AddressId} retrieved successfully for Customer {CustomerId}.", addressId, customerId);

            var response = ApiResponse<CustomerAddressResponseDTO>
                    .SuccessResponse(
                        result,
                        "Address retrieved successfully.");

            return Ok(response);
        }

        // Create a new Address for the authenticated Customer.
        // Endpoint: POST api/addresses
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CustomerAddressResponseDTO>>> CreateAddress(
                [FromBody] CreateCustomerAddressRequestDTO request)
        {
            var customerId = GetCustomerId();

            _logger.LogInformation("Create Address request received for Customer {CustomerId}.", customerId);

            var result = await _addressService.CreateAddressAsync(customerId, request);

            _logger.LogInformation("Address {AddressId} created successfully for Customer {CustomerId}.", result.Id, customerId);

            var response = ApiResponse<CustomerAddressResponseDTO>
                    .SuccessResponse(
                        result,
                        "Address created successfully.");

            return Ok(response);
        }

        // Update an existing Address belonging to the authenticated Customer.
        // Endpoint: PUT api/addresses/{addressId}
        [HttpPut("{addressId:int}")]
        public async Task<ActionResult<ApiResponse<CustomerAddressResponseDTO>>> UpdateAddress(
                int addressId,
                [FromBody] UpdateCustomerAddressRequestDTO request)
        {
            var customerId = GetCustomerId();

            _logger.LogInformation(
                "Updating Address {AddressId} for Customer {CustomerId}.",
                addressId,
                customerId);

            var result = await _addressService.UpdateAddressAsync(customerId, addressId, request);

            _logger.LogInformation(
                "Address {AddressId} updated successfully for Customer {CustomerId}.",
                addressId,
                customerId);

            var response = ApiResponse<CustomerAddressResponseDTO>
                    .SuccessResponse(
                        result,
                        "Address updated successfully.");

            return Ok(response);
        }

        // Delete an existing Address belonging to the authenticated Customer.
        // Endpoint: DELETE api/addresses/{addressId}
        [HttpDelete("{addressId:int}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteAddress(int addressId)
        {
            var customerId = GetCustomerId();

            _logger.LogInformation("Deleting Address {AddressId} for Customer {CustomerId}.", addressId, customerId);

            await _addressService.DeleteAddressAsync(customerId, addressId);

            _logger.LogInformation("Address {AddressId} deleted successfully for Customer {CustomerId}.", addressId, customerId);

            var response = ApiResponse<string>.SuccessResponse(
                    "Address deleted.",
                    "Address deleted successfully.");

            return Ok(response);
        }
    }
}
