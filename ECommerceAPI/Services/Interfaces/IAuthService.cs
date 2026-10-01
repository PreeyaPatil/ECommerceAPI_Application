using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<CustomerResponseDTO> RegisterAsync(CustomerRegistrationRequestDTO request);
        Task VerifyEmailAsync(EmailVerificationRequestDTO request);
        Task VerifyMobileAsync(MobileVerificationRequestDTO request);
        Task ResendEmailVerificationCodeAsync(ResendEmailVerificationCodeRequestDTO request);
        Task ResendMobileVerificationCodeAsync(ResendMobileVerificationCodeRequestDTO request);
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<TokenResponseDTO> VerifyTwoFactorAsync(TwoFactorLoginVerificationRequestDTO request);
        Task<TokenResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO request);
        Task LogoutAsync(LogoutRequestDTO request);
        Task EnableTwoFactorAsync(int customerId);
        Task DisableTwoFactorAsync(int customerId);
    }
}
