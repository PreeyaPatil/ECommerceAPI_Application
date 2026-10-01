using ECommerceAPI.Repositories.Implementations;
using ECommerceAPI.Repositories.Interfaces;

namespace ECommerceAPI.Extensions
{
    public static class RepositoryExtensions
    {
        // Registers all repository services with the Dependency Injection container.
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Registers CustomerRepository for customer-related database operations.
            services.AddScoped<ICustomerRepository, CustomerRepository>();

            // Registers VerificationCodeRepository for verification code operations.
            services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();

            // Registers RefreshTokenRepository for refresh token operations.
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Registers ProductRepository for product and category operations.
            services.AddScoped<IProductRepository, ProductRepository>();

            // Registers CartRepository for shopping cart operations.
            services.AddScoped<ICartRepository, CartRepository>();

            // Registers CustomerAddressRepository for customer address operations.
            services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();

            // Registers OrderRepository for order-related database operations.
            services.AddScoped<IOrderRepository, OrderRepository>();

            // Registers PaymentRepository for payment-related database operations.
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            // Registers NotificationRepository for notification queue operations.
            services.AddScoped<INotificationRepository, NotificationRepository>();

            // Registers UnitOfWork for saving changes and managing database transactions.
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Returns the IServiceCollection so additional services can be registered.
            return services;
        }
    }
}
