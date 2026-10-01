using ECommerceAPI.Entities;
namespace ECommerceAPI.Repositories.Interfaces
{
    public interface ICartRepository
    {
        // Gets a customer's cart with its items and product details
        // for read-only operations.
        Task<Cart?> GetByCustomerIdAsync(int customerId);

        // Gets a customer's cart with its items and product details
        // with tracking enabled for update operations.
        Task<Cart?> GetByCustomerIdForUpdateAsync(int customerId);

        // Adds a new cart to the database context.
        Task AddAsync(Cart cart);

        // Removes a specific item from the cart.
        void RemoveItem(CartItem cartItem);

        // Removes all specified items from the cart.
        void ClearItems(IEnumerable<CartItem> cartItems);
    }
}
