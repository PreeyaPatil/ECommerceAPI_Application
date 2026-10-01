namespace ECommerceAPI.Extensions
{
    // Contains extension methods for configuring OpenAPI documentation and Swagger UI.
    public static class SwaggerExtensions
    {
        // Registers the services required to generate
        // the OpenAPI document for the application.

        // This is an extension method for IServiceCollection,
        // so it can be called from Program.cs as:
        // builder.Services.AddSwaggerDocumentation();
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            // Registers ASP.NET Core's built-in OpenAPI support.
            //
            // "v1" is the name of the OpenAPI document.
            // Specifying it explicitly makes the document name
            // clear instead of relying on the default value.
            //
            // ASP.NET Core uses information about the application's
            // endpoints, routes, HTTP methods, request models,
            // response models, and other metadata to generate
            // the OpenAPI specification.
            services.AddOpenApi("v1");

            // Return the same IServiceCollection instance
            // so that additional service registrations can continue to be chained.
            return services;
        }

        // Configures the OpenAPI endpoint and Swagger UI.
        // This is an extension method for WebApplication,
        // so it can be called from Program.cs as:
        // app.UseSwaggerDocumentation();
        public static WebApplication UseSwaggerDocumentation(this WebApplication app)
        {
            // Enable OpenAPI and Swagger UI only when the
            // application is running in the Development environment.
            //
            // This prevents Swagger UI from being exposed
            // automatically in Production.
            if (app.Environment.IsDevelopment())
            {
                // Maps an endpoint that exposes the generated
                // OpenAPI specification as a JSON document.
                //
                // Since the OpenAPI document was registered with the name "v1",
                // the generated document will be available at:
                // /openapi/v1.json
                app.MapOpenApi();

                // Adds Swagger UI to the application.
                // Swagger UI provides an interactive web interface
                // where developers can:
                // - View available API endpoints
                // - View HTTP methods
                // - View request and response models
                // - Provide request parameters
                // - Execute API requests directly from the browser
                // - Inspect API responses
                app.UseSwaggerUI(options =>
                {
                    // Configure Swagger UI to use the OpenAPI document generated
                    // by ASP.NET Core.
                    // First parameter:
                    // "/openapi/v1.json"
                    //     The URL of the generated OpenAPI document.
                    //
                    // Second parameter:
                    // "E-Commerce API v1"
                    //     The friendly name displayed inside Swagger UI.
                    options.SwaggerEndpoint(
                        "/openapi/v1.json",
                        "E-Commerce API v1");

                    // Specifies the URL path used to access Swagger UI.
                    // With the RoutePrefix set to "swagger",
                    // Swagger UI will be available at:
                    // /swagger
                    // Example:
                    // https://localhost:5001/swagger
                    options.RoutePrefix = "swagger";
                });
            }

            // Return the same WebApplication instance
            // so that additional middleware configuration
            // can continue in Program.cs.
            return app;
        }
    }
}
