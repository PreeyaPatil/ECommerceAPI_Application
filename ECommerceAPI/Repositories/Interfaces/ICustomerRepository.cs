using ECommerceAPI.Entities;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        // Gets a customer by their unique customer ID.
        Task<Customer?> GetByIdAsync(int customerId);

        // Gets a customer by ID with change tracking enabled for update operations.
        Task<Customer?> GetByIdForUpdateAsync(int customerId);

        // Gets a customer using their email address.
        Task<Customer?> GetByEmailAsync(string email);

        // Gets a customer using their phone number.
        Task<Customer?> GetByPhoneNumberAsync(string phoneNumber);

        // Checks whether a customer already exists with the given email address.
        Task<bool> EmailExistsAsync(string email);

        // Checks whether a customer already exists with the given phone number.
        Task<bool> PhoneNumberExistsAsync(string phoneNumber);

        // Adds a new customer to the database context.
        Task AddAsync(Customer customer);
    }
}
