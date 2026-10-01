using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Models;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<PagedResult<ProductListItemResponseDTO>> GetProductsAsync(ProductFilterRequestDTO request);
        Task<ProductDetailsResponseDTO> GetProductByIdAsync(int productId);
        Task<List<CategoryResponseDTO>> GetCategoriesAsync();
    }
}
