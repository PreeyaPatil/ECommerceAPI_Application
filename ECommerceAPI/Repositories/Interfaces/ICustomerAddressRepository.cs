using ECommerceAPI.Entities;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface ICustomerAddressRepository
    {
        // Gets all addresses of a customer for read-only operations.
        Task<List<CustomerAddress>> GetByCustomerIdAsync(int customerId);

        // Gets all addresses of a customer with tracking enabled for update operations.
        Task<List<CustomerAddress>> GetByCustomerIdForUpdateAsync(int customerId);

        // Gets a specific customer address by address ID and customer ID for read-only operations.
        Task<CustomerAddress?> GetByIdAsync(int addressId, int customerId);

        // Gets a specific customer address with tracking enabled so that it can be updated.
        Task<CustomerAddress?> GetByIdForUpdateAsync(int addressId, int customerId);

        // Adds a new customer address to the database context.
        Task AddAsync(CustomerAddress address);

        // Removes the specified customer address from the database context.
        void Remove(CustomerAddress address);
    }
}
