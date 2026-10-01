using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/products")]
    [AllowAnonymous]
    public sealed class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // Retrieve Products using the supplied search,
        // filtering, sorting, and paging criteria.
        // Endpoint: GET api/products
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<ProductListItemResponseDTO>>>> GetProducts(
                [FromQuery] ProductFilterRequestDTO request)
        {
            _logger.LogInformation(
                "Product List request received for Page {PageNumber} with Page Size {PageSize}.",
                request.PageNumber,
                request.PageSize);

            var result = await _productService.GetProductsAsync(request);

            _logger.LogInformation("Product List retrieved successfully for Page {PageNumber}.", request.PageNumber);

            var response = ApiResponse<PagedResult<ProductListItemResponseDTO>>
                    .SuccessResponse(
                        result,
                        "Products retrieved successfully.");

            return Ok(response);
        }

        // Retrieve complete information for
        // a specific active Product.
        // Endpoint: GET api/products/{productId}
        [HttpGet("{productId:int}")]
        public async Task<ActionResult<ApiResponse<ProductDetailsResponseDTO>>> GetProduct(int productId)
        {
            _logger.LogInformation("Product Details request received for Product {ProductId}.", productId);

            var result = await _productService.GetProductByIdAsync(productId);

            _logger.LogInformation("Product {ProductId} retrieved successfully.", productId);

            var response = ApiResponse<ProductDetailsResponseDTO>
                    .SuccessResponse(
                        result,
                        "Product retrieved successfully.");

            return Ok(response);
        }

        // Retrieve all active Product Categories
        // that can be used for browsing and filtering Products.
        // Endpoint: GET api/products/categories
        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<List<CategoryResponseDTO>>>> GetCategories()
        {
            _logger.LogInformation("Product Categories request received.");

            var result = await _productService.GetCategoriesAsync();

            _logger.LogInformation("Product Categories retrieved successfully.");

            var response = ApiResponse<List<CategoryResponseDTO>>
                    .SuccessResponse(
                        result,
                        "Categories retrieved successfully.");

            return Ok(response);
        }
    }
}
