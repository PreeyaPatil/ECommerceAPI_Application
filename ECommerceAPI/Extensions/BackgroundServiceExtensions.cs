using ECommerceAPI.BackgroundServices;

namespace ECommerceAPI.Extensions
{
    public static class BackgroundServiceExtensions
    {
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            // Register NotificationProcessor as an ASP.NET Core Hosted Service.
            // It will automatically start when the application starts
            // and stop when the application stops.
            services.AddHostedService<NotificationProcessor>();

            return services;
        }
    }
}
