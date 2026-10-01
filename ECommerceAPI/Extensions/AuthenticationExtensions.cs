using ECommerceAPI.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ECommerceAPI.Extensions
{
    public static class AuthenticationExtensions
    {
        // Registers and configures JWT Bearer Authentication
        // using values from the application's configuration.
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Read the JWT configuration section.
            // Example:
            // "Jwt": {
            //   "Key": "...",
            //   "Issuer": "...",
            //   "Audience": "...",
            //   "AccessTokenMinutes": 30
            // }
            var jwtSection = configuration.GetSection(JwtOptions.SectionName);

            // Convert the configuration section
            // into a strongly typed JwtOptions object.
            // If the section cannot be loaded,
            // stop application startup with a clear error.
            var jwtOptions =
                jwtSection.Get<JwtOptions>()
                ?? throw new InvalidOperationException("JWT configuration is missing.");

            // Ensure that the JWT signing key has been configured.
            if (string.IsNullOrWhiteSpace(jwtOptions.Key))
            {
                throw new InvalidOperationException("JWT signing key is missing.");
            }

            // Ensure that the signing key is at least 32 bytes long.
            if (Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
            {
                throw new InvalidOperationException("JWT signing key must be at least 32 bytes.");
            }

            // Register JwtOptions with the Options pattern.
            // This allows JwtOptions to be injected later using
            // IOptions<JwtOptions> or similar abstractions.
            services.Configure<JwtOptions>(jwtSection);

            // Register authentication services
            // and configure JWT Bearer as the default scheme.
            services
                .AddAuthentication(options =>
                {
                    // Use JWT Bearer authentication when ASP.NET Core
                    // tries to authenticate an incoming request.
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                    // Use JWT Bearer authentication when access
                    // to a protected resource must be challenged.
                    // For example, an unauthenticated request to an
                    // [Authorize] endpoint will normally receive 401 Unauthorized.
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })

                // Configure how incoming JWT access tokens should be validated.
                .AddJwtBearer(options =>
                {
                    // Require HTTPS when retrieving authentication metadata.
                    // This is recommended for secure environments.
                    options.RequireHttpsMetadata = true;

                    // Define the rules that every incoming JWT must satisfy before it is accepted.
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Verify that the token was issued by the expected issuer.
                        ValidateIssuer = true,

                        // The issuer in the token must match the configured application issuer.
                        ValidIssuer = jwtOptions.Issuer,

                        // Verify that the token is intended for the expected audience.
                        ValidateAudience = true,

                        // The token audience must match the configured application audience.
                        ValidAudience = jwtOptions.Audience,

                        // Verify that the token has not expired
                        // and is currently within its valid lifetime.
                        ValidateLifetime = true,

                        // Verify the token's digital signature
                        // using the configured signing key.
                        ValidateIssuerSigningKey = true,

                        // Create the symmetric security key
                        // from the configured JWT secret key.
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
                    };
                });

            // Register authorization services.
            // This enables features such as the [Authorize] attribute
            // and authorization policies.
            services.AddAuthorization();

            // Return IServiceCollection so additional services
            // can be registered using method chaining.
            return services;
        }
    }
}
