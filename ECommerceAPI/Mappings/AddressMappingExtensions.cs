using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;

namespace ECommerceAPI.Mappings
{
    public static class AddressMappingExtensions
    {
        // Converts a create-address request DTO into a CustomerAddress entity.
        // CustomerAddress entity = request.ToEntity(customerId);
        public static CustomerAddress ToEntity(
            this CreateCustomerAddressRequestDTO request, 
            int customerId)
        {
            return new CustomerAddress
            {
                // Associate the address with the specified customer.
                CustomerId = customerId,

                // Copy the address details from the request DTO.
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,

                // Set whether this address is the default shipping or billing address.
                IsDefaultShipping = request.IsDefaultShipping,
                IsDefaultBilling = request.IsDefaultBilling
            };
        }

        // Updates an existing CustomerAddress entity with the values from the request DTO.
        // request: Updated Entity
        // address: Existing Entity
        // request.MapToEntity(address)
        public static void MapToEntity(
            this UpdateCustomerAddressRequestDTO request, 
            CustomerAddress address)
        {
            // Update the address details.
            address.FullName = request.FullName;
            address.PhoneNumber = request.PhoneNumber;
            address.AddressLine1 = request.AddressLine1;
            address.AddressLine2 = request.AddressLine2;
            address.City = request.City;
            address.State = request.State;
            address.PostalCode = request.PostalCode;
            address.Country = request.Country;

            // Update the default shipping and billing settings.
            address.IsDefaultShipping = request.IsDefaultShipping;
            address.IsDefaultBilling = request.IsDefaultBilling;
        }

        // Converts a CustomerAddress entity into a response DTO.
        public static CustomerAddressResponseDTO ToResponseDTO(this CustomerAddress address)
        {
            return new CustomerAddressResponseDTO
            {
                // Copy the required address information to the response DTO.
                Id = address.Id,
                FullName = address.FullName,
                PhoneNumber = address.PhoneNumber,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,
                IsDefaultShipping = address.IsDefaultShipping,
                IsDefaultBilling = address.IsDefaultBilling
            };
        }
    }
}
