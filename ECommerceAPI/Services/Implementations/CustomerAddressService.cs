using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Mappings;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services.Implementations
{
    public sealed class CustomerAddressService : ICustomerAddressService
    {
        // Used to retrieve, create, update, and delete
        // Customer Address data from the database.
        private readonly ICustomerAddressRepository _addressRepository;

        // Used to save all database changes.
        private readonly IUnitOfWork _unitOfWork;

        // Used to write application logs.
        private readonly ILogger<CustomerAddressService> _logger;

        public CustomerAddressService(
            ICustomerAddressRepository addressRepository,
            IUnitOfWork unitOfWork,
            ILogger<CustomerAddressService> logger)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<CustomerAddressResponseDTO>> GetAddressesAsync(int customerId)
        {
            // Retrieve all saved Addresses for the Customer.
            var addresses = await _addressRepository.GetByCustomerIdAsync(customerId);

            // Convert Address Entities into Response DTOs.
            return addresses
                .Select(x => x.ToResponseDTO())
                .ToList();
        }

        public async Task<CustomerAddressResponseDTO> GetAddressAsync(int customerId, int addressId)
        {
            // Retrieve the specific Address that belongs to the given Customer.
            var address = await _addressRepository.GetByIdAsync(addressId, customerId);

            // If the Address does not exist or does not belong to the Customer, return a Not Found error.
            if (address == null)
            {
                _logger.LogWarning(
                    "Address {AddressId} was not found for Customer {CustomerId}.",
                    addressId,
                    customerId);

                throw new NotFoundException("Address was not found.");
            }

            // Convert the Address Entity into Response DTO.
            return address.ToResponseDTO();
        }

        public async Task<CustomerAddressResponseDTO> CreateAddressAsync(
            int customerId, 
            CreateCustomerAddressRequestDTO request)
        {
            // Retrieve all Customer Addresses as tracked Entities
            // because some existing default flags may need to be changed.
            var existingAddresses = await _addressRepository.GetByCustomerIdForUpdateAsync(customerId);

            // If the new Address should become the default Shipping Address,
            // remove the default flag from all existing Addresses.
            if (request.IsDefaultShipping)
            {
                foreach (var address in existingAddresses)
                {
                    address.IsDefaultShipping = false;
                }
            }

            // If the new Address should become the default Billing Address,
            // remove the default flag from all existing Addresses.
            if (request.IsDefaultBilling)
            {
                foreach (var address in existingAddresses)
                {
                    address.IsDefaultBilling = false;
                }
            }

            // Convert the Request DTO into a new CustomerAddress Entity.
            var newAddress = request.ToEntity(customerId);

            // Add the new Address to the database context.
            await _addressRepository.AddAsync(newAddress);

            // Save both the existing Address updates and the newly created Address.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Address {AddressId} created successfully for Customer {CustomerId}.",
                newAddress.Id,
                customerId);

            // Return the newly created Address.
            return newAddress.ToResponseDTO();
        }

        public async Task<CustomerAddressResponseDTO> UpdateAddressAsync(
            int customerId,
            int addressId,
            UpdateCustomerAddressRequestDTO request)
        {
            // Retrieve all Customer Addresses as tracked Entities
            // because the selected Address and default flags may be updated.
            var addresses = await _addressRepository.GetByCustomerIdForUpdateAsync(customerId);

            // Find the Address that should be updated.
            var address = addresses.FirstOrDefault(x => x.Id == addressId);

            if (address == null)
            {
                throw new NotFoundException("Address was not found.");
            }

            // If this Address should become the default Shipping Address,
            // clear the flag from all Addresses first.
            if (request.IsDefaultShipping)
            {
                foreach (var existingAddress in addresses)
                {
                    existingAddress.IsDefaultShipping = false;
                }
            }

            // If this Address should become the default Billing Address,
            // clear the flag from all Addresses first.
            if (request.IsDefaultBilling)
            {
                foreach (var existingAddress in addresses)
                {
                    existingAddress.IsDefaultBilling = false;
                }
            }

            // Copy the updated values from the Request DTO
            // into the existing tracked Address Entity.
            request.MapToEntity(address);

            // Save the Address changes.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Address {AddressId} updated successfully for Customer {CustomerId}.",
                addressId,
                customerId);

            // Return the updated Address.
            return address.ToResponseDTO();
        }

        public async Task DeleteAddressAsync(int customerId, int addressId)
        {
            // Retrieve the Address as a tracked Entity because it will be deleted.
            var address = await _addressRepository.GetByIdForUpdateAsync(addressId, customerId);

            if (address == null)
            {
                throw new NotFoundException("Address was not found.");
            }

            // Mark the Address for deletion.
            _addressRepository.Remove(address);

            // Save the deletion.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Address {AddressId} deleted successfully for Customer {CustomerId}.",
                addressId, customerId);
        }
    }
}
