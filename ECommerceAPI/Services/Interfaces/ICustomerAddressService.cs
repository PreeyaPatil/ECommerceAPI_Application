using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;

namespace ECommerceAPI.Services.Interfaces
{
    public interface ICustomerAddressService
    {
        Task<List<CustomerAddressResponseDTO>> GetAddressesAsync(int customerId);

        Task<CustomerAddressResponseDTO> GetAddressAsync(int customerId, int addressId);

        Task<CustomerAddressResponseDTO> CreateAddressAsync(int customerId, CreateCustomerAddressRequestDTO request);

        Task<CustomerAddressResponseDTO> UpdateAddressAsync(
                int customerId,
                int addressId,
                UpdateCustomerAddressRequestDTO request);

        Task DeleteAddressAsync(int customerId, int addressId);
    }
}
