using ECommerceAPI.Data;
using ECommerceAPI.Entities;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Repositories.Implementations
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly ECommerceDbContext _context;
        public OrderRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        // Adds a new order to the DbContext for insertion into the database.
        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        // Gets a paginated list of orders placed by the specified customer.
        public async Task<PagedResult<Order>> GetCustomerOrdersAsync(
            int customerId,
            int pageNumber,
            int pageSize)
        {
            // Creates a read-only query for the customer's orders with related order information.
            var query = _context.Orders
                .AsNoTracking()
                .Include(x => x.PaymentMethod)
                .Include(x => x.PaymentStatus)
                .Include(x => x.OrderStatus)
                .Include(x => x.Items)
                .Where(x => x.CustomerId == customerId);

            // Gets the total number of orders before paging is applied.
            var totalCount = await query.CountAsync();

            // Retrieves only the orders for the requested page, showing the latest orders first.
            var orders = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Returns the orders along with paging information.
            return new PagedResult<Order>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = orders
            };
        }

        // Gets complete details of a specific order belonging to the specified customer.
        public async Task<Order?> GetDetailsAsync(int orderId, int customerId)
        {
            return await _context.Orders
                .AsNoTracking()

                // Loads the payment method selected for the order.
                .Include(x => x.PaymentMethod)

                // Loads the current payment status of the order.
                .Include(x => x.PaymentStatus)

                // Loads the current order status.
                .Include(x => x.OrderStatus)

                // Loads all items purchased in the order.
                .Include(x => x.Items)

                // Loads payment records along with their payment methods.
                .Include(x => x.Payments)
                    .ThenInclude(x => x.PaymentMethod)

                // Loads payment records along with their payment statuses.
                .Include(x => x.Payments)
                    .ThenInclude(x => x.PaymentStatus)

                // Loads order status history along with the previous status.
                .Include(x => x.StatusHistories)
                    .ThenInclude(x => x.PreviousStatus)

                // Loads order status history along with the new status.
                .Include(x => x.StatusHistories)
                    .ThenInclude(x => x.NewStatus)

                // Ensures the requested order belongs to the specified customer.
                .FirstOrDefaultAsync(x =>
                    x.Id == orderId &&
                    x.CustomerId == customerId);
        }

        // Gets an order by ID with tracking enabled
        // so that the order and related data can be updated.
        public async Task<Order?> GetByIdForUpdateAsync(int orderId)
        {
            return await _context.Orders
                .Include(x => x.Items)
                .Include(x => x.Payments)
                .Include(x => x.StatusHistories)
                .FirstOrDefaultAsync(x => x.Id == orderId);
        }

        // Adds a new status history record to track an order status change.
        public async Task AddStatusHistoryAsync(OrderStatusHistory orderStatusHistory)
        {
            await _context.OrderStatusHistories
                .AddAsync(orderStatusHistory);
        }

        // Gets an existing order using the customer ID and checkout token.
        public async Task<Order?> GetByCheckoutTokenAsync(int customerId, Guid checkoutToken)
        {
            return await _context.Orders
                .Include(x => x.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                         x => x.CustomerId == customerId &&
                         x.CheckoutToken == checkoutToken);
        }
    }
}
