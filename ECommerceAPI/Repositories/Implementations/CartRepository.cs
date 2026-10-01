using ECommerceAPI.Data;
using ECommerceAPI.Entities;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Repositories.Implementations
{
    public sealed class CartRepository : ICartRepository
    {
        private readonly ECommerceDbContext _context;

        public CartRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        // Gets a customer's cart with its items and product details
        // without tracking the entities.
        public async Task<Cart?> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Carts
                .AsNoTracking()
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.CustomerId == customerId);
        }

        // Gets a customer's cart with its items and product details
        // with tracking enabled so that it can be updated.
        public async Task<Cart?> GetByCustomerIdForUpdateAsync(int customerId)
        {
            return await _context.Carts
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.CustomerId == customerId);
        }

        // Adds a new cart to the DbContext for insertion into the database.
        public async Task AddAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
        }

        // Marks the specified cart item for removal from the database.
        public void RemoveItem(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
        }

        // Marks all specified cart items for removal from the database.
        public void ClearItems(IEnumerable<CartItem> cartItems)
        {
            _context.CartItems.RemoveRange(cartItems);
        }
    }
}
