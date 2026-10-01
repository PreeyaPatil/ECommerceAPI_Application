using ECommerceAPI.Options;
using ECommerceAPI.Services.Implementations;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register JWT configuration settings from appsettings.json.
            // These settings are later injected through IOptions<JwtOptions>.
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

            // Register Verification configuration settings,
            // such as Verification Code expiry and maximum failed attempts.
            services.Configure<VerificationOptions>(configuration.GetSection(VerificationOptions.SectionName));

            // Register Pricing configuration settings,
            // such as Tax, Shipping Charge, Currency, and Free Shipping Threshold.
            services.Configure<PricingOptions>(configuration.GetSection(PricingOptions.SectionName));

            // Register Notification configuration settings,
            // such as Batch Size, Max Retries, and Processing Timeout.
            services.Configure<NotificationOptions>(configuration.GetSection(NotificationOptions.SectionName));

            // Register TokenService for JWT Access Token and Refresh Token related operations.
            services.AddScoped<ITokenService, TokenService>();

            // Register AuthService for Customer Registration,
            // Login, Verification, 2FA, Refresh Token, and Logout operations.
            services.AddScoped<IAuthService, AuthService>();

            // Register ProductService for Product Listing,
            // Product Details, Filtering, Sorting, and Categories.
            services.AddScoped<IProductService, ProductService>();

            // Register CartService for Shopping Cart operations.
            services.AddScoped<ICartService, CartService>();

            // Register CustomerAddressService for Customer Address management.
            services.AddScoped<ICustomerAddressService, CustomerAddressService>();

            // Register CheckoutService for preparing Checkout information before Order Placement.
            services.AddScoped<ICheckoutService, CheckoutService>();

            // Register PaymentService for creating and updating Payment Transactions.
            services.AddScoped<IPaymentService, PaymentService>();

            // Register PaymentSimulator for simulating Card and UPI payment results.
            services.AddScoped<IPaymentSimulator, PaymentSimulator>();

            // Register NotificationService for queuing Email and SMS Notifications.
            services.AddScoped<INotificationService, NotificationService>();

            // Register OrderService for Order Placement,
            // Order History, and Order Details operations.
            services.AddScoped<IOrderService, OrderService>();

            // Return IServiceCollection so that
            // additional services can be registered if required.
            return services;
        }
    }
}
