using ECommerceAPI.Data;
using ECommerceAPI.Entities;
using ECommerceAPI.Enums;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Repositories.Implementations
{
    public sealed class PaymentRepository : IPaymentRepository
    {
        private readonly ECommerceDbContext _context;
        public PaymentRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        // Gets all active payment methods ordered by their configured display order.
        public async Task<List<PaymentMethodMaster>> GetActivePaymentMethodsAsync()
        {
            return await _context.PaymentMethods
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        // Gets a specific active payment method using the provided PaymentMethod enum value.
        public async Task<PaymentMethodMaster?> GetPaymentMethodAsync(PaymentMethod paymentMethod)
        {
            return await _context.PaymentMethods
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == paymentMethod &&
                    x.IsActive);
        }

        // Gets the most recently created payment transaction for the specified order.
        public async Task<PaymentTransaction?> GetLatestByOrderIdAsync(int orderId)
        {
            return await _context.PaymentTransactions
                .Where(x => x.OrderId == orderId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync();
        }

        // Adds a new payment transaction to the DbContext for insertion into the database.
        public async Task AddAsync(PaymentTransaction paymentTransaction)
        {
            await _context.PaymentTransactions
                .AddAsync(paymentTransaction);
        }
    }
}
