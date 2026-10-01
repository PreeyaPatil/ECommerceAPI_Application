using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;

namespace ECommerceAPI.Services.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResponseDTO> GenerateTokensAsync(Customer customer);
        Task<TokenResponseDTO> RefreshTokensAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }
}
