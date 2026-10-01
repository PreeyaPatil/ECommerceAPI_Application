using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;

namespace ECommerceAPI.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDTO> GetCartAsync(int customerId);

        Task<CartResponseDTO> AddToCartAsync(int customerId, AddToCartRequestDTO request);

        Task<CartResponseDTO> UpdateQuantityAsync(
            int customerId,
            int cartItemId,
            UpdateCartItemQuantityRequestDTO request);

        Task RemoveItemAsync(int customerId, int cartItemId);

        Task ClearCartAsync(int customerId);
    }
}
