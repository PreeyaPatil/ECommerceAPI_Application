using ECommerceAPI.Entities;
namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        // Gets a refresh token using its hashed token value.
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);

        // Adds a new refresh token to the database context.
        Task AddAsync(RefreshToken refreshToken);
    }
}
