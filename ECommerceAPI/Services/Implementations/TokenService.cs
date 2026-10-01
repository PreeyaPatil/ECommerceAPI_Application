using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;
using ECommerceAPI.Helpers;
using ECommerceAPI.Mappings;
using ECommerceAPI.Options;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ECommerceAPI.Services.Implementations
{
    public sealed class TokenService : ITokenService
    {
        // Used to store, retrieve, and update Refresh Tokens.
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        // Used to retrieve Customer information.
        private readonly ICustomerRepository _customerRepository;

        // Used to save all database changes.
        private readonly IUnitOfWork _unitOfWork;

        // Contains JWT configuration values such as
        // Secret Key, Issuer, Audience, Token Expiry, etc.
        private readonly JwtOptions _jwtOptions;

        // Used to write application logs.
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            IRefreshTokenRepository refreshTokenRepository,
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            IOptions<JwtOptions> jwtOptions,
            ILogger<TokenService> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;

            // Read JwtOptions values from appsettings.json.
            _jwtOptions = jwtOptions.Value;

            _logger = logger;
        }

        // Creates Access Token + Refresh Token
        public async Task<TokenResponseDTO> GenerateTokensAsync(Customer customer)
        {
            _logger.LogInformation(
                "Generating Access and Refresh Tokens for Customer {CustomerId}.",
                customer.Id);

            // Generate the JWT Access Token.
            var jwtResult = JwtTokenHelper.GenerateAccessToken(customer, _jwtOptions);

            // Generate a secure Refresh Token.
            // The result contains both the plain token and the hashed token.
            var refreshTokenResult = RefreshTokenHelper.Generate(_jwtOptions);

            // Convert the Refresh Token result into a RefreshToken Entity.
            // Only the hashed token is stored in the database.
            var refreshToken = refreshTokenResult.ToEntity(customer.Id);

            // Add the Refresh Token Entity to the database.
            await _refreshTokenRepository.AddAsync(refreshToken);

            // Save the Refresh Token.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Access and Refresh Tokens generated successfully for Customer {CustomerId}.",
                customer.Id);

            // Return the Access Token and the plain Refresh Token to the client.
            return jwtResult.ToResponseDTO(refreshTokenResult);
        }

        // Validates old Refresh Token
        // Revokes old token
        // Creates new Access Token + new Refresh Token
        public async Task<TokenResponseDTO> RefreshTokensAsync(string refreshToken)
        {
            _logger.LogInformation("Refresh Token rotation requested.");

            // Hash the Refresh Token received from the client.
            // The database stores only the hashed version.
            var tokenHash = TokenHashHelper.HashToken(refreshToken);

            // Find the stored Refresh Token using its hash.
            var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            // Reject the request if the Refresh Token does not exist in the database.
            if (existingToken == null)
            {
                _logger.LogWarning("Refresh Token validation failed. Token was not found.");
                throw new UnauthorizedAccessException("Invalid Refresh Token.");
            }

            // Reject the Refresh Token if it has already been revoked.
            if (existingToken.RevokedAtUtc.HasValue)
            {
                _logger.LogWarning(
                    "Refresh Token for Customer {CustomerId} has already been revoked.",
                    existingToken.CustomerId);

                throw new UnauthorizedAccessException("Refresh Token has already been revoked.");
            }

            // Reject the Refresh Token if it has expired.
            if (existingToken.ExpiresAtUtc <= DateTime.UtcNow)
            {
                _logger.LogWarning(
                    "Refresh Token for Customer {CustomerId} has expired.",
                    existingToken.CustomerId);

                throw new UnauthorizedAccessException("Refresh Token has expired.");
            }

            // Load the Customer associated with this Refresh Token.
            var customer = await _customerRepository.GetByIdAsync(existingToken.CustomerId);

            // Reject token refresh if the Customer no longer exists or the account is inactive.
            if (customer == null || !customer.IsActive)
            {
                _logger.LogWarning(
                    "Token refresh failed because Customer {CustomerId} is not available.",
                    existingToken.CustomerId);

                throw new UnauthorizedAccessException("Customer account is not available.");
            }

            // Revoke the old Refresh Token.
            // This implements Refresh Token rotation.
            RefreshTokenHelper.Revoke(existingToken);

            // Generate a new JWT Access Token.
            var jwtResult = JwtTokenHelper.GenerateAccessToken(customer, _jwtOptions);

            // Generate a new Refresh Token.
            var newRefreshTokenResult = RefreshTokenHelper.Generate(_jwtOptions);

            // Convert the new Refresh Token result into a database Entity.
            var newRefreshToken = newRefreshTokenResult.ToEntity(customer.Id);

            // Store the new Refresh Token.
            await _refreshTokenRepository.AddAsync(newRefreshToken);

            // Save both changes:
            // 1. Old Refresh Token revoked
            // 2. New Refresh Token created
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Refresh Token rotated successfully for Customer {CustomerId}.",
                customer.Id);

            // Return the newly generated Access Token and Refresh Token to the client.
            return jwtResult.ToResponseDTO(newRefreshTokenResult);
        }

        // Revokes the Refresh Token
        // Used during Logout
        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("Refresh Token revocation requested.");

            // Hash the Refresh Token received from the client.
            var tokenHash = TokenHashHelper.HashToken(refreshToken);

            // Find the matching Refresh Token in the database.
            var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            // If the token does not exist, the request is invalid.
            if (existingToken == null)
            {
                _logger.LogWarning("Refresh Token revocation failed. Token was not found.");

                throw new UnauthorizedAccessException("Invalid Refresh Token.");
            }

            // If the Refresh Token is already revoked, no further action is required.
            if (existingToken.RevokedAtUtc.HasValue)
            {
                _logger.LogInformation(
                    "Refresh Token for Customer {CustomerId} is already revoked.",
                    existingToken.CustomerId);

                return;
            }

            // Mark the Refresh Token as revoked.
            RefreshTokenHelper.Revoke(existingToken);

            // Save the revocation in the database.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Refresh Token revoked successfully for Customer {CustomerId}.",
                existingToken.CustomerId);
        }
    }
}
