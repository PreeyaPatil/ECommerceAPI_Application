using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;
using ECommerceAPI.Models;

namespace ECommerceAPI.Mappings
{
    public static class AuthenticationMappingExtensions
    {
        // Converts a RefreshTokenResult into a RefreshToken entity for database storage.
        public static RefreshToken ToEntity(
            this RefreshTokenResult refreshTokenResult, 
            int customerId)
        {
            return new RefreshToken
            {
                // Associate the refresh token with the authenticated customer.
                CustomerId = customerId,

                // Store the hashed refresh token instead of the plain-text token.
                TokenHash = refreshTokenResult.TokenHash,

                // Store the date and time when the refresh token will expire.
                ExpiresAtUtc = refreshTokenResult.ExpiresAtUtc
            };
        }

        // Combines the generated JWT access token and refresh token information
        // into a TokenResponseDTO that will be returned to the client.
        public static TokenResponseDTO ToResponseDTO(
            this JwtTokenResult jwtTokenResult, 
            RefreshTokenResult refreshTokenResult)
        {
            return new TokenResponseDTO
            {
                // Access token used by the client to access protected API endpoints.
                AccessToken = jwtTokenResult.AccessToken,

                // Expiration date and time of the access token.
                AccessTokenExpiresAtUtc = jwtTokenResult.ExpiresAtUtc,

                // Return the plain-text refresh token to the client.
                RefreshToken = refreshTokenResult.PlainTextToken,

                // Expiration date and time of the refresh token.
                RefreshTokenExpiresAtUtc = refreshTokenResult.ExpiresAtUtc
            };
        }
    }
}
