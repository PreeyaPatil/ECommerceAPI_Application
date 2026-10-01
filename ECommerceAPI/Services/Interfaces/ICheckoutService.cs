using ECommerceAPI.DTOs.Responses;
namespace ECommerceAPI.Services.Interfaces
{
    public interface ICheckoutService
    {
        Task<CheckoutResponseDTO> GetCheckoutAsync(int customerId);
    }
}
