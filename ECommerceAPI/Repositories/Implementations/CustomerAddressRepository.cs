using ECommerceAPI.Data;
using ECommerceAPI.Entities;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Repositories.Implementations
{
    public sealed class CustomerAddressRepository : ICustomerAddressRepository
    {
        private readonly ECommerceDbContext _context;
        public CustomerAddressRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        // Gets all addresses of a customer without tracking and orders default addresses first.
        public async Task<List<CustomerAddress>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.CustomerAddresses
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.IsDefaultShipping)
                .ThenByDescending(x => x.IsDefaultBilling)
                .ThenByDescending(x => x.CreatedAtUtc)
                .ToListAsync();
        }

        // Gets all addresses of a customer with tracking enabled so that they can be updated.
        public async Task<List<CustomerAddress>> GetByCustomerIdForUpdateAsync(int customerId)
        {
            return await _context.CustomerAddresses
                .Where(x => x.CustomerId == customerId)
                .ToListAsync();
        }

        // Gets a specific address belonging to the specified customer without tracking the entity.
        public async Task<CustomerAddress?> GetByIdAsync(int addressId, int customerId)
        {
            return await _context.CustomerAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == addressId &&
                    x.CustomerId == customerId);
        }

        // Gets a specific address belonging to the specified customer with tracking enabled for updates.
        public async Task<CustomerAddress?> GetByIdForUpdateAsync(int addressId, int customerId)
        {
            return await _context.CustomerAddresses
                .FirstOrDefaultAsync(x =>
                    x.Id == addressId &&
                    x.CustomerId == customerId);
        }

        // Adds a new customer address to the DbContext for insertion into the database.
        public async Task AddAsync(CustomerAddress address)
        {
            await _context.CustomerAddresses.AddAsync(address);
        }

        // Marks the specified customer address for deletion from the database.
        public void Remove(CustomerAddress address)
        {
            _context.CustomerAddresses.Remove(address);
        }
    }
}
