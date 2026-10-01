using ECommerceAPI.Data;
using ECommerceAPI.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ECommerceAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create the WebApplicationBuilder.
            // This loads configuration, logging, DI, environment information, etc.
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog as the application's logging provider.
            // Serilog configuration is loaded from appsettings.json
            // through the AddSerilogLogging extension method.
            builder.AddSerilogLogging();

            // Register Controllers.
            builder.Services.AddControllers();

            // Register OpenAPI support.
            // Microsoft.AspNetCore.OpenApi generates the OpenAPI document
            // that will later be displayed through Swagger UI.
            builder.Services.AddSwaggerDocumentation();

            // Register Entity Framework Core and SQL Server.
            builder.Services.AddDbContext<ECommerceDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("ECommerceDBConnection"));
            });

            // Register all Repository interfaces and implementations.
            builder.Services.AddRepositories();

            // Register the Global Exception Handler.
            builder.Services.AddGlobalExceptionHandling();

            // Register JWT Authentication and Authorization services.
            builder.Services.AddJwtAuthentication(builder.Configuration);

            // Register Email and SMS services
            // along with their configuration options.
            builder.Services.AddCommunicationServices(builder.Configuration);

            // Register application/business services.
            builder.Services.AddApplicationServices(builder.Configuration);

            // Register Background Services.
            builder.Services.AddBackgroundServices();

            // Build the application.
            var app = builder.Build();

            // Adds ASP.NET Core's TraceIdentifier to Serilog LogContext.
            // All log statements generated while processing the same HTTP
            // request can therefore contain the same TraceIdentifier.
            app.UseTraceIdentifierLogging();

            // Logs one structured summary entry for every HTTP request.
            // Typical information includes request path, status code, and execution time.
            app.UseSerilogRequestLogging();

            // Handles unhandled exceptions using the registered Global Exception Handler.
            app.UseExceptionHandler();

            // Configure OpenAPI and Swagger UI only in Development.
            if (app.Environment.IsDevelopment())
            {
                // Exposes the generated OpenAPI document
                // and displays it using Swagger UI.
                // OpenAPI document: /openapi/v1.json
                // Swagger UI: /swagger
                app.UseSwaggerDocumentation();
            }

            // Redirect HTTP requests to HTTPS.
            app.UseHttpsRedirection();

            // Reads and validates authentication information
            // such as JWT Bearer Tokens.
            app.UseAuthentication();

            // Applies authorization rules such as [Authorize].
            app.UseAuthorization();

            // Maps Controller endpoints.
            app.MapControllers();

            // Start the application.
            app.Run();
        }
    }
}
