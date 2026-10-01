using ECommerceAPI.Entities;
using ECommerceAPI.Enums;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        // Gets all active payment methods available for the customer.
        Task<List<PaymentMethodMaster>> GetActivePaymentMethodsAsync();

        // Gets a specific active payment method using the PaymentMethod enum value.
        Task<PaymentMethodMaster?> GetPaymentMethodAsync(PaymentMethod paymentMethod);

        // Gets the latest payment transaction created for a specific order.
        Task<PaymentTransaction?> GetLatestByOrderIdAsync(int orderId);

        // Adds a new payment transaction to the database context.
        Task AddAsync(PaymentTransaction paymentTransaction);
    }
}
