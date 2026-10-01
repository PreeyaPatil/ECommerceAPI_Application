using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [Route("api/auth")]
    public sealed class AuthController : AuthenticatedControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // Register a new Customer account and start
        // the Email and Mobile verification process.
        // Endpoint: POST api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<CustomerResponseDTO>>> Register(
            [FromBody] CustomerRegistrationRequestDTO request)
        {
            _logger.LogInformation("Customer Registration request received.");

            var result = await _authService.RegisterAsync(request);

            _logger.LogInformation("Customer Registration completed successfully.");

            var response = ApiResponse<CustomerResponseDTO>
                    .SuccessResponse(
                        result,
                        "Registration completed successfully. Please verify your Email and Mobile Number.");

            return Ok(response);
        }

        // Verify the Customer's Email Address
        // using the verification code provided by the Customer.
        // Endpoint: POST api/auth/verify-email
        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> VerifyEmail(
            [FromBody] EmailVerificationRequestDTO request)
        {
            _logger.LogInformation("Email Verification request received.");

            await _authService.VerifyEmailAsync(request);

            _logger.LogInformation("Email Verification completed successfully.");

            var response = ApiResponse<string>.SuccessResponse(
                    "Email verified successfully.",
                    "Email verified successfully.");

            return Ok(response);
        }

        // Verify the Customer's Mobile Number
        // using the verification code provided by the Customer.
        // Endpoint: POST api/auth/verify-mobile
        [HttpPost("verify-mobile")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> VerifyMobile(
            [FromBody] MobileVerificationRequestDTO request)
        {
            _logger.LogInformation("Mobile Verification request received.");

            await _authService.VerifyMobileAsync(request);

            _logger.LogInformation("Mobile Verification completed successfully.");

            var response = ApiResponse<string>.SuccessResponse(
                    "Mobile Number verified successfully.",
                    "Mobile Number verified successfully.");

            return Ok(response);
        }

        // Generate and send a new Email Verification Code
        // when the previous code is unavailable or expired.
        // Endpoint: POST api/auth/resend-email-verification-code
        [HttpPost("resend-email-verification-code")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> ResendEmailVerificationCode(
                [FromBody] ResendEmailVerificationCodeRequestDTO request)
        {
            _logger.LogInformation("Resend Email Verification Code request received.");

            await _authService.ResendEmailVerificationCodeAsync(request);

            _logger.LogInformation("Email Verification Code resent successfully.");

            var response = ApiResponse<string>.SuccessResponse(
                    "Verification code sent successfully.",
                    "Email verification code sent successfully.");

            return Ok(response);
        }

        // Generate and send a new Mobile Verification Code
        // when the previous code is unavailable or expired.
        // Endpoint: POST api/auth/resend-mobile-verification-code
        [HttpPost("resend-mobile-verification-code")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> ResendMobileVerificationCode(
                [FromBody] ResendMobileVerificationCodeRequestDTO request)
        {
            _logger.LogInformation("Resend Mobile Verification Code request received.");

            await _authService.ResendMobileVerificationCodeAsync(request);

            _logger.LogInformation("Mobile Verification Code resent successfully.");

            var response = ApiResponse<string>.SuccessResponse(
                    "Verification code sent successfully.",
                    "Mobile verification code sent successfully.");

            return Ok(response);
        }

        // Authenticate the Customer using Email and Password.
        // If 2FA is enabled, the Customer must complete
        // the Two-Factor Authentication step before Tokens are issued.
        // Endpoint: POST api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login(
            [FromBody] LoginRequestDTO request)
        {
            _logger.LogInformation("Customer Login request received.");

            var result = await _authService.LoginAsync(request);

            _logger.LogInformation(
                "Customer Login request processed. Requires Two-Factor Authentication: {RequiresTwoFactor}.",
                result.RequiresTwoFactor);

            var response = ApiResponse<LoginResponseDTO>
                    .SuccessResponse(
                        result,
                        result.RequiresTwoFactor
                            ? "Two-Factor Authentication verification is required."
                            : "Login successful.");

            return Ok(response);
        }

        // Complete the Login process by validating
        // the Two-Factor Authentication verification code.
        // Endpoint: POST api/auth/verify-two-factor
        [HttpPost("verify-two-factor")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TokenResponseDTO>>> VerifyTwoFactor(
                [FromBody] TwoFactorLoginVerificationRequestDTO request)
        {
            _logger.LogInformation("Two-Factor Authentication verification request received.");

            var result = await _authService.VerifyTwoFactorAsync(request);

            _logger.LogInformation("Two-Factor Authentication verified successfully.");

            var response = ApiResponse<TokenResponseDTO>
                    .SuccessResponse(
                        result,
                        "Two-Factor Authentication verified successfully.");

            return Ok(response);
        }

        // Generate a new Access Token and Refresh Token
        // using Refresh Token rotation.
        // Endpoint: POST api/auth/refresh-token
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TokenResponseDTO>>> RefreshToken(
                [FromBody] RefreshTokenRequestDTO request)
        {
            // Never log the actual Refresh Token.
            _logger.LogInformation("Refresh Token request received.");

            var result = await _authService.RefreshTokenAsync(request);

            _logger.LogInformation("Refresh Token rotation completed successfully.");

            var response = ApiResponse<TokenResponseDTO>
                    .SuccessResponse(
                        result,
                        "Tokens refreshed successfully.");

            return Ok(response);
        }

        // Logout the Customer by revoking
        // the supplied Refresh Token.
        // Endpoint: POST api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> Logout(
            [FromBody] LogoutRequestDTO request)
        {
            var customerId = GetCustomerId();

            _logger.LogInformation("Logout request received for Customer {CustomerId}.", customerId);

            await _authService.LogoutAsync(request);

            _logger.LogInformation("Customer {CustomerId} logged out successfully.", customerId);

            var response = ApiResponse<string>.SuccessResponse(
                    "Logout successful.",
                    "Logout successful.");

            return Ok(response);
        }

        // Enable Email-based Two-Factor Authentication
        // for the authenticated Customer.
        // Endpoint: POST api/auth/two-factor/enable
        [HttpPost("two-factor/enable")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> EnableTwoFactor()
        {
            var customerId = GetCustomerId();

            _logger.LogInformation("Enable Two-Factor Authentication request received for Customer {CustomerId}.", customerId);

            await _authService.EnableTwoFactorAsync(customerId);

            _logger.LogInformation("Two-Factor Authentication enabled successfully for Customer {CustomerId}.", customerId);

            var response = ApiResponse<string>.SuccessResponse(
                    "Two-Factor Authentication enabled.",
                    "Two-Factor Authentication enabled successfully.");

            return Ok(response);
        }

        // Disable Two-Factor Authentication
        // for the authenticated Customer.
        // Endpoint: POST api/auth/two-factor/disable
        [HttpPost("two-factor/disable")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> DisableTwoFactor()
        {
            var customerId = GetCustomerId();

            _logger.LogInformation("Disable Two-Factor Authentication request received for Customer {CustomerId}.", customerId);

            await _authService.DisableTwoFactorAsync(customerId);

            _logger.LogInformation("Two-Factor Authentication disabled successfully for Customer {CustomerId}.", customerId);

            var response = ApiResponse<string>.SuccessResponse(
                    "Two-Factor Authentication disabled.",
                    "Two-Factor Authentication disabled successfully.");

            return Ok(response);
        }
    }
}
