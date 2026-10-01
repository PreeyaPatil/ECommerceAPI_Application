using ECommerceAPI.Data;
using ECommerceAPI.Entities;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Repositories.Implementations
{
    public sealed class ProductRepository : IProductRepository
    {
        private readonly ECommerceDbContext _context;
        public ProductRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        // Gets a paginated list of active products based on
        // search, filter, sorting, and paging criteria.
        public async Task<PagedResult<Product>> GetProductsAsync(
            string? searchTerm,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            bool inStockOnly,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize)
        {
            // Creates a read-only query for active products along with their category details.
            var query = _context.Products
                .AsNoTracking()
                .Include(x => x.Category)
                .Where(x => x.IsActive)
                .AsQueryable();

            // Searches products by Name, Description, or SKU.
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(x =>
                    x.Name.Contains(searchTerm) ||
                    x.Description.Contains(searchTerm) ||
                    x.Sku.Contains(searchTerm));
            }

            // Filters products by the selected category.
            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            // Filters products whose price is greater than or equal to the minimum price.
            if (minPrice.HasValue)
            {
                query = query.Where(x => x.Price >= minPrice.Value);
            }

            // Filters products whose price is less than or equal to the maximum price.
            if (maxPrice.HasValue)
            {
                query = query.Where(x => x.Price <= maxPrice.Value);
            }

            // Filters only products that are currently available in stock.
            if (inStockOnly)
            {
                query = query.Where(x => x.StockQuantity > 0);
            }

            // Gets the total number of matching products before paging is applied.
            var totalCount = await query.CountAsync();

            // Determines whether the requested sorting direction is descending.
            var descending =
                string.Equals(
                    sortDirection,
                    "desc",
                    StringComparison.OrdinalIgnoreCase);

            // Sorts products by the requested field and direction.
            switch (sortBy?.Trim().ToLowerInvariant())
            {
                case "price":
                    query = descending
                        ? query.OrderByDescending(x => x.Price)
                        : query.OrderBy(x => x.Price);
                    break;

                case "name":
                default:
                    query = descending
                        ? query.OrderByDescending(x => x.Name)
                        : query.OrderBy(x => x.Name);
                    break;
            }

            // Applies paging and retrieves only the products for the requested page.
            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Returns the paginated products along with paging information.
            return new PagedResult<Product>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = products
            };
        }

        // Gets an active product by ID along with its category without tracking the entity.
        public async Task<Product?> GetByIdAsync(int productId)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(x => x.Category)
                .FirstOrDefaultAsync(
                    x => x.Id == productId &&
                         x.IsActive);
        }

        // Gets all active categories ordered by display order and then by category name.
        public async Task<List<Category>> GetActiveCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }

        // Gets active products by their unique IDs with tracking enabled
        // so that they can be updated.
        public async Task<List<Product>> GetProductsByIdsForUpdateAsync(List<int> productIds)
        {
            // Removes duplicate product IDs before querying the database.
            var ids = productIds
                .Distinct()
                .ToList();

            return await _context.Products
                .Where(x => ids.Contains(x.Id) && x.IsActive)
                .ToListAsync();
        }
    }
}
