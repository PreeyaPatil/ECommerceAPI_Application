using ECommerceAPI.Entities;
using ECommerceAPI.Models;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IProductRepository
    {
        // Gets a paginated list of active products based on
        // search, filter, sorting, and paging criteria.
        Task<PagedResult<Product>> GetProductsAsync(
            string? searchTerm,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            bool inStockOnly,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize);

        // Gets an active product by its unique product ID.
        Task<Product?> GetByIdAsync(int productId);

        // Gets all active product categories.
        Task<List<Category>> GetActiveCategoriesAsync();

        // Gets active products by their IDs with tracking enabled for update operations.
        Task<List<Product>> GetProductsByIdsForUpdateAsync(List<int> productIds);
    }
}

