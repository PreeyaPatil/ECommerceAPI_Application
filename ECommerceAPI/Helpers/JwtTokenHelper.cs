using ECommerceAPI.Entities;
using ECommerceAPI.Models;
using ECommerceAPI.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerceAPI.Helpers
{
    public static class JwtTokenHelper
    {
        // Generates a signed JWT access token for the given customer.
        // The generated token contains basic customer information
        // as claims and is valid for the configured duration.
        public static JwtTokenResult GenerateAccessToken(Customer customer, JwtOptions options)
        {
            // Capture the current UTC time.
            // This will be used as the token's starting time
            // and also to calculate its expiration time.
            var issuedAtUtc = DateTime.UtcNow;

            // Calculate when the access token should expire.
            // Example:
            // AccessTokenMinutes = 30
            // The token will expire 30 minutes after it is issued.
            var expiresAtUtc = issuedAtUtc.AddMinutes(options.AccessTokenMinutes);

            // Create the claims that will be stored inside the JWT.
            // Claims represent information about the authenticated user.
            var claims = new List<Claim>
            {
                // Store the customer's unique ID in the token.
                // This can later be used to identify
                // which customer is making the request.
                new(ClaimTypes.NameIdentifier, customer.Id.ToString()),

                // Store the customer's email address in the token.
                new(ClaimTypes.Email, customer.Email),

                // Jti stands for JWT ID.
                // A new unique value is generated for every token,
                // which helps uniquely identify a particular JWT.
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Convert the secret key from configuration into a security key
            // that can be used for signing the JWT.
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));

            // Create the signing credentials.
            // The signature helps the API verify that
            // the token has not been modified after it was issued.
            var signingCredentials =
                new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Create the JWT with all required information.
            var token = new JwtSecurityToken(
                    // Identifies who issued the token.
                    issuer: options.Issuer,

                    // Identifies who is allowed to use the token.
                    audience: options.Audience,

                    // Add customer-related information to the token.
                    claims: claims,

                    // Token should not be accepted before this time.
                    notBefore: issuedAtUtc,

                    // Token should not be accepted after this time.
                    expires: expiresAtUtc,

                    // Sign the token using the configured secret key.
                    signingCredentials: signingCredentials);

            // JwtSecurityTokenHandler converts the JWT object
            // into the final compact token string.
            var tokenHandler = new JwtSecurityTokenHandler();

            // Return the generated access token together with its expiration time.
            return new JwtTokenResult
            {
                // Convert the JwtSecurityToken object
                // into a string that can be sent to the client.
                AccessToken = tokenHandler.WriteToken(token),

                // Return the expiration time so the client
                // knows when the access token will expire.
                ExpiresAtUtc = expiresAtUtc
            };
        }
    }
}
