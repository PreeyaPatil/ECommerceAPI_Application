using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Models;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IOrderService
    {
        Task<PlaceOrderResponseDTO> PlaceOrderAsync(int customerId, PlaceOrderRequestDTO request);
        Task<PagedResult<OrderListItemResponseDTO>> GetOrderHistoryAsync(int customerId, OrderHistoryRequestDTO request);
        Task<OrderDetailsResponseDTO> GetOrderDetailsAsync(int customerId, int orderId);
    }
}

