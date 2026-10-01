using ECommerceAPI.Entities;
using ECommerceAPI.Models;
using ECommerceAPI.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace ECommerceAPI.Helpers
{
    public static class RefreshTokenHelper
    {
        // Generates a new secure refresh token along with its hash and expiration date.
        public static RefreshTokenResult Generate(JwtOptions options)
        {
            // Generate 64 cryptographically secure random bytes.
            var randomBytes = RandomNumberGenerator.GetBytes(64);

            // Convert the random bytes into a Base64 URL-safe string.
            // Base64UrlEncoder avoids characters such as '+', '/' and '=',
            // making the token safe to use in HTTP requests, JSON, or URLs.
            // This is the actual refresh token that will be sent to the client.
            var plainTextToken = Base64UrlEncoder.Encode(randomBytes);

            // Hash the refresh token before storing it in the database.
            // The original refresh token should not be stored directly.
            // Later, when the client sends the token back, we can hash it again
            // and compare it with the stored hash.
            var tokenHash = TokenHashHelper.HashToken(plainTextToken);

            // Return all required refresh token information.
            return new RefreshTokenResult
            {
                // Original token returned to the client.
                PlainTextToken = plainTextToken,

                // Hashed token that should be stored in the database.
                TokenHash = tokenHash,

                // Set the refresh token expiration date
                // based on the configured number of valid days.
                // Example:
                // RefreshTokenDays = 7
                // The token will expire 7 days from the current UTC time.
                ExpiresAtUtc = DateTime.UtcNow.AddDays(options.RefreshTokenDays)
            };
        }

        // Revokes an existing refresh token.
        // Once revoked, the token should no longer be accepted
        // for generating a new access token.
        public static void Revoke(RefreshToken refreshToken)
        {
            // Capture the current UTC time once so that
            // both properties receive exactly the same timestamp.
            var utcNow = DateTime.UtcNow;

            // Record when the refresh token was revoked.
            refreshToken.RevokedAtUtc = utcNow;

            // Update the entity's last modified timestamp.
            refreshToken.UpdatedAtUtc = utcNow;
        }
    }
}
