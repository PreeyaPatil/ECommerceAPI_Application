using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;
using ECommerceAPI.Models;

namespace ECommerceAPI.Mappings
{
    public static class ProductMappingExtensions
    {
        // Converts a Category entity into a CategoryResponseDTO.
        public static CategoryResponseDTO ToResponseDTO(this Category category)
        {
            return new CategoryResponseDTO
            {
                // Copy the required category information to the response DTO.
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        // Converts a Product entity into a lightweight DTO used in product listing pages.
        public static ProductListItemResponseDTO ToListItemResponseDTO(this Product product)
        {
            return new ProductListItemResponseDTO
            {
                // Copy the basic product information.
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                Price = product.Price,
                StockQuantity = product.StockQuantity,

                // Product is considered in stock when available quantity is greater than zero.
                IsInStock = product.StockQuantity > 0,

                ImageUrl = product.ImageUrl,

                // Include basic category information with the product.
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name
            };
        }

        // Converts a Product entity into a detailed response DTO.
        public static ProductDetailsResponseDTO ToDetailsResponseDTO(this Product product)
        {
            return new ProductDetailsResponseDTO
            {
                // Copy the complete product information.
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Sku = product.Sku,
                Price = product.Price,
                StockQuantity = product.StockQuantity,

                // Determine the stock availability based on the current quantity.
                IsInStock = product.StockQuantity > 0,

                ImageUrl = product.ImageUrl,

                // Convert the related Category entity into CategoryResponseDTO.
                Category = product.Category.ToResponseDTO()
            };
        }

        // Converts a paginated collection of Product entities into a paginated
        // collection of ProductListItemResponseDTO objects.
        public static PagedResult<ProductListItemResponseDTO> ToProductListPagedResult(this PagedResult<Product> pagedResult)
        {
            return new PagedResult<ProductListItemResponseDTO>
            {
                // Preserve the pagination information from the original result.
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount,

                // Convert each Product entity into a ProductListItemResponseDTO.
                Items = pagedResult.Items
                    .Select(product => product.ToListItemResponseDTO())
                    .ToList()
            };
        }
    }
}
