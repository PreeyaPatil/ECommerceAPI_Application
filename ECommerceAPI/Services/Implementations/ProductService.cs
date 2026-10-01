using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Mappings;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services.Implementations
{
    public sealed class ProductService : IProductService
    {
        // Used to retrieve Product and Category data from the database.
        private readonly IProductRepository _productRepository;

        // Used to write application logs.
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<PagedResult<ProductListItemResponseDTO>> GetProductsAsync(ProductFilterRequestDTO request)
        {
            // Log the paging information for the Product Listing request.
            _logger.LogInformation(
                "Retrieving Products for Page {PageNumber} with Page Size {PageSize}.",
                request.PageNumber,
                request.PageSize);

            // Validate the price range.
            // Minimum Price should not be greater than Maximum Price.
            if (request.MinPrice.HasValue &&
                request.MaxPrice.HasValue &&
                request.MinPrice.Value > request.MaxPrice.Value)
            {
                _logger.LogWarning("Invalid Product price filter. Minimum Price is greater than Maximum Price.");

                throw new BusinessException("Minimum Price cannot be greater than Maximum Price.");
            }

            // Retrieve Products from the Repository using
            // search, filtering, sorting, and paging values.
            var result =
                await _productRepository
                    .GetProductsAsync(
                        request.SearchTerm,
                        request.CategoryId,
                        request.MinPrice,
                        request.MaxPrice,
                        request.InStockOnly,
                        request.SortBy,
                        request.SortDirection,
                        request.PageNumber,
                        request.PageSize);

            _logger.LogInformation(
                "Products retrieved successfully for Page {PageNumber}.",
                request.PageNumber);

            // Convert the paged Product Entities into
            // paged Product List Response DTOs.
            return result.ToProductListPagedResult();
        }

        public async Task<ProductDetailsResponseDTO> GetProductByIdAsync(int productId)
        {
            _logger.LogInformation("Retrieving Product {ProductId}.", productId);

            // Retrieve the Product from the database.
            var product = await _productRepository.GetByIdAsync(productId);

            // The Product must exist and also be active.
            // Inactive Products should not be exposed to Customers.
            if (product == null || !product.IsActive)
            {
                _logger.LogWarning(
                    "Product {ProductId} was not found or is inactive.",
                    productId);

                throw new NotFoundException("Product was not found.");
            }

            _logger.LogInformation(
                "Product {ProductId} retrieved successfully.",
                productId);

            // Convert the Product Entity into
            // ProductDetailsResponseDTO.
            return product.ToDetailsResponseDTO();
        }

        public async Task<List<CategoryResponseDTO>> GetCategoriesAsync()
        {
            // Retrieve only active Product Categories.
            var categories = await _productRepository.GetActiveCategoriesAsync();

            _logger.LogInformation("Active Product Categories retrieved successfully.");

            // Convert Category Entities into
            // CategoryResponseDTO objects.
            return categories
                .Select(x => x.ToResponseDTO())
                .ToList();
        }
    }
}
