using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;

namespace ECommerceAPI.Mappings
{
    // Contains extension methods used to map Customer-related DTOs and entities.
    public static class CustomerMappingExtensions
    {
        // Converts CustomerRegistrationRequestDTO into a Customer entity.
        // Customer entity = request.ToEntity();
        public static Customer ToEntity(this CustomerRegistrationRequestDTO request)
        {
            return new Customer
            {
                // Copy the customer information received from the registration request.
                FirstName = request.FirstName,
                LastName = request.LastName,
                //Email = request.Email,
                //PhoneNumber = request.PhoneNumber,

                // New customers are initially created with unconfirmed email and mobile.
                IsEmailConfirmed = false,
                IsMobileConfirmed = false,

                // Two-factor authentication is disabled by default.
                IsTwoFactorEnabled = false,

                // New customers are active by default.
                IsActive = true
            };
        }

        // Converts a Customer entity into CustomerResponseDTO.
        // CustomerResponseDTO dto = customer.ToResponseDTO();
        public static CustomerResponseDTO ToResponseDTO(this Customer customer)
        {
            return new CustomerResponseDTO
            {
                // Copy the required customer information from the entity
                // into the response DTO that will be returned to the client.
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                IsEmailConfirmed = customer.IsEmailConfirmed,
                IsMobileConfirmed = customer.IsMobileConfirmed,
                IsTwoFactorEnabled = customer.IsTwoFactorEnabled,
                IsActive = customer.IsActive,
                CreatedAtUtc = customer.CreatedAtUtc
            };
        }
    }
}
