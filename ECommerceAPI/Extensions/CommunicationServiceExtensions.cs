using ECommerceAPI.Options;
using ECommerceAPI.Services.Implementations;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Extensions
{
    public static class CommunicationServiceExtensions
    {
        // IServiceCollection: Represents the application's Dependency Injection container.
        //
        // IConfiguration: Provides access to configuration values from sources such as
        //                 appsettings.json, environment variables, and user secrets.
        public static IServiceCollection AddCommunicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind the "Email" section from appsettings.json
            // to the strongly typed EmailOptions class.
            // After this registration, EmailOptions can be accessed
            // using IOptions<EmailOptions>.
            services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

            // Bind the "Twilio" section from appsettings.json
            // to the strongly typed TwilioOptions class.
            // After this registration, TwilioOptions can be accessed
            // using IOptions<TwilioOptions>.
            services.Configure<TwilioOptions>(configuration.GetSection(TwilioOptions.SectionName));

            // Register EmailService as the implementation of IEmailService.
            // Whenever IEmailService is requested through Dependency Injection,
            // ASP.NET Core will provide an instance of EmailService.
            services.AddSingleton<IEmailService, EmailService>();

            // Register SmsService as the implementation of ISmsService.
            // Whenever ISmsService is requested through Dependency Injection,
            // ASP.NET Core will provide an instance of SmsService.
            services.AddSingleton<ISmsService, SmsService>();

            // Return IServiceCollection so that additional service
            // registrations can be chained if required.
            return services;
        }
    }
}
