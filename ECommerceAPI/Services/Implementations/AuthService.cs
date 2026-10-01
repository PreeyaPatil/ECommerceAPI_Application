using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;
using ECommerceAPI.Enums;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Helpers;
using ECommerceAPI.Mappings;
using ECommerceAPI.Options;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerceAPI.Services.Implementations
{
    public sealed class AuthService : IAuthService
    {
        // Used to perform Customer-related database operations.
        private readonly ICustomerRepository _customerRepository;

        // Used to store and retrieve verification codes.
        private readonly IVerificationCodeRepository _verificationCodeRepository;

        // Used to save all database changes.
        private readonly IUnitOfWork _unitOfWork;

        // Used to generate, refresh, and revoke JWT and Refresh Tokens.
        private readonly ITokenService _tokenService;

        // Used to send Email messages.
        private readonly IEmailService _emailService;

        // Used to send SMS messages.
        private readonly ISmsService _smsService;

        // Contains verification code settings such as
        // expiry time and maximum failed attempts.
        private readonly VerificationOptions _verificationOptions;

        // Used to write application logs.
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ICustomerRepository customerRepository,
            IVerificationCodeRepository verificationCodeRepository,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IEmailService emailService,
            ISmsService smsService,
            IOptions<VerificationOptions> verificationOptions,
            ILogger<AuthService> logger)
        {
            _customerRepository = customerRepository;
            _verificationCodeRepository = verificationCodeRepository;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _emailService = emailService;
            _smsService = smsService;

            // Read VerificationOptions values from configuration.
            _verificationOptions = verificationOptions.Value;

            _logger = logger;
        }

        public async Task<CustomerResponseDTO> RegisterAsync(CustomerRegistrationRequestDTO request)
        {
            _logger.LogInformation("Customer Registration started.");

            // Normalize the Email before storing or comparing it.
            var email = request.Email.Trim().ToLowerInvariant();

            // Normalize the Mobile Number into a consistent format.
            var phoneNumber = NormalizePhoneNumber(request.PhoneNumber);

            // Check whether the Email Address is already registered.
            if (await _customerRepository.EmailExistsAsync(email))
            {
                _logger.LogWarning("Customer Registration failed because the Email Address is already registered.");
                throw new ConflictException("Email Address is already registered.");
            }

            // Check whether the Mobile Number is already registered.
            if (await _customerRepository.PhoneNumberExistsAsync(phoneNumber))
            {
                _logger.LogWarning("Customer Registration failed because the Mobile Number is already registered.");
                throw new ConflictException("Mobile Number is already registered.");
            }

            // Convert the Registration Request DTO into a Customer Entity.
            var customer = request.ToEntity();

            // Store the normalized values.
            customer.Email = email;
            customer.PhoneNumber = phoneNumber;

            // Hash the Password before storing it in the database.
            customer.PasswordHash = PasswordHelper.HashPassword(customer, request.Password);

            // Add the Customer Entity to the DbContext.
            await _customerRepository.AddAsync(customer);

            try
            {
                // Save the new Customer to the database.
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Customer {CustomerId} registered successfully.", customer.Id);
            }
            catch (DbUpdateException)
            {
                _logger.LogWarning("A database conflict occurred while registering a Customer.");

                // Recheck Email because another request may
                // have inserted the same Email concurrently.
                if (await _customerRepository.EmailExistsAsync(email))
                {
                    throw new ConflictException("Email Address is already registered.");
                }

                // Recheck Mobile Number for the same reason.
                if (await _customerRepository.PhoneNumberExistsAsync(phoneNumber))
                {
                    throw new ConflictException("Mobile Number is already registered.");
                }

                // If the problem is unrelated to Email or Mobile duplication,
                // rethrow the original database exception.
                throw;
            }

            // Create an Email verification code.
            var emailCode =
                await CreateVerificationCodeAsync(
                    VerificationPurpose.EmailVerification,
                    customer.Email,
                    customer.Id,
                    _verificationOptions.CodeExpiryMinutes);

            // Create a Mobile verification code.
            var mobileCode =
                await CreateVerificationCodeAsync(
                    VerificationPurpose.MobileVerification,
                    customer.PhoneNumber,
                    customer.Id,
                    _verificationOptions.CodeExpiryMinutes);

            // Prepare the Email body.
            var emailBody = $"""
                <h3>Email Verification</h3>

                <p>Hello {customer.FirstName},</p>

                <p>Your Email verification code is:</p>

                <h2>{emailCode}</h2>

                <p>
                    This code is valid for
                    {_verificationOptions.CodeExpiryMinutes} minutes.
                </p>
                """;

            // Send the Email verification code.
            await _emailService.SendEmailAsync(customer.Email, "Email Verification", emailBody, isHtml:true);

            var smsBody = $"Your E-Commerce Mobile verification code is " +
                $"{mobileCode}. It is valid for " +
                $"{_verificationOptions.CodeExpiryMinutes} minutes.";

            // Send the Mobile verification code through SMS.
            await _smsService.SendSmsAsync(customer.PhoneNumber, smsBody);

            _logger.LogInformation(
                "Email and Mobile verification messages sent for Customer {CustomerId}.",
                customer.Id);

            // Return the registered Customer information.
            return customer.ToResponseDTO();
        }

        public async Task VerifyEmailAsync(EmailVerificationRequestDTO request)
        {
            // Validate the submitted Email verification code.
            var verificationCode = await ValidateVerificationCodeAsync(
                    VerificationPurpose.EmailVerification,
                    request.Email,
                    request.Code,
                    _verificationOptions.MaxFailedAttempts);

            // Load the Customer as a tracked Entity
            // because we are going to update it.
            var customer = await _customerRepository.GetByIdForUpdateAsync(verificationCode.CustomerId);

            if (customer == null)
            {
                throw new NotFoundException("Customer was not found.");
            }

            // If already verified, no further update is required.
            if (customer.IsEmailConfirmed)
            {
                _logger.LogInformation(
                    "Email verified successfully for Customer {CustomerId}.",
                    customer.Id);

                return;
            }

            // Mark the Email Address as verified.
            customer.IsEmailConfirmed = true;
            customer.UpdatedAtUtc = DateTime.UtcNow;

            // Save Customer update and verification code changes.
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task VerifyMobileAsync(MobileVerificationRequestDTO request)
        {
            // Validate the submitted Mobile verification code.
            var verificationCode =
                await ValidateVerificationCodeAsync(
                    VerificationPurpose.MobileVerification,
                    NormalizePhoneNumber(request.PhoneNumber),
                    request.Code,
                    _verificationOptions.MaxFailedAttempts);

            // Load the Customer for update.
            var customer =
                await _customerRepository.GetByIdForUpdateAsync(verificationCode.CustomerId);

            if (customer == null)
            {
                throw new NotFoundException("Customer was not found.");
            }

            // If already verified, no further update is required.
            if (customer.IsMobileConfirmed)
            {
                _logger.LogInformation(
                    "Mobile Number verified successfully for Customer {CustomerId}.",
                    customer.Id);

                return;
            }

            // Mark the Mobile Number as verified.
            customer.IsMobileConfirmed = true;
            customer.UpdatedAtUtc = DateTime.UtcNow;

            // Save Customer update and verification code changes.
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ResendEmailVerificationCodeAsync(ResendEmailVerificationCodeRequestDTO request)
        {
            // Normalize the Email Address.
            var email = request.Email.Trim().ToLowerInvariant();

            // Find the Customer using Email.
            var customer = await _customerRepository.GetByEmailAsync(email);

            if (customer == null)
            {
                throw new NotFoundException("Customer was not found.");
            }

            // Inactive Customers should not receive verification codes.
            if (!customer.IsActive)
            {
                throw new BusinessException("Customer account is inactive.");
            }

            // No need to resend if Email is already verified.
            if (customer.IsEmailConfirmed)
            {
                throw new BusinessException("Email Address is already verified.");
            }

            // Generate and store a new Email verification code.
            var code =
                await CreateVerificationCodeAsync(
                    VerificationPurpose.EmailVerification,
                    customer.Email,
                    customer.Id,
                    _verificationOptions.CodeExpiryMinutes);

            // Prepare the Email body.
            var emailBody = $"""
                <h3>Email Verification</h3>

                <p>Hello {customer.FirstName},</p>

                <p>Your new Email verification code is:</p>

                <h2>{code}</h2>

                <p>
                    This code is valid for
                    {_verificationOptions.CodeExpiryMinutes} minutes.
                </p>
                """;

            // Send the new verification code.
            await _emailService.SendEmailAsync(customer.Email, "Email Verification Code", emailBody);

            _logger.LogInformation(
                "Email verification code resent for Customer {CustomerId}.",
                customer.Id);
        }

        public async Task ResendMobileVerificationCodeAsync(ResendMobileVerificationCodeRequestDTO request)
        {
            // Normalize the Mobile Number.
            var phoneNumber = NormalizePhoneNumber(request.PhoneNumber);

            // Find the Customer using Mobile Number.
            var customer = await _customerRepository.GetByPhoneNumberAsync(phoneNumber);

            if (customer == null)
            {
                throw new NotFoundException("Customer was not found.");
            }

            // Inactive Customers should not receive verification codes.
            if (!customer.IsActive)
            {
                throw new BusinessException("Customer account is inactive.");
            }

            // No need to resend if Mobile Number is already verified.
            if (customer.IsMobileConfirmed)
            {
                throw new BusinessException("Mobile Number is already verified.");
            }

            // Generate and store a new Mobile verification code.
            var code =
                await CreateVerificationCodeAsync(
                    VerificationPurpose.MobileVerification,
                    customer.PhoneNumber,
                    customer.Id,
                    _verificationOptions.CodeExpiryMinutes);

            // Send the new verification code through SMS.
            await _smsService.SendSmsAsync(
                customer.PhoneNumber,
                $"Your new E-Commerce Mobile verification " +
                $"code is {code}. It is valid for " +
                $"{_verificationOptions.CodeExpiryMinutes} minutes.");

            _logger.LogInformation(
                "Mobile verification code resent for Customer {CustomerId}.",
                customer.Id);
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            _logger.LogInformation("Customer Login attempt started.");

            // Normalize the Email Address.
            var email = request.Email.Trim().ToLowerInvariant();

            // Find the Customer using Email.
            var customer = await _customerRepository.GetByEmailAsync(email);

            // Do not reveal whether the Email exists.
            if (customer == null)
            {
                _logger.LogWarning("Customer Login failed due to invalid credentials.");

                throw new UnauthorizedAccessException("Invalid Email or Password.");
            }

            // Verify the submitted Password against the stored Password Hash.
            var validPassword = PasswordHelper.VerifyPassword(customer, customer.PasswordHash, request.Password);

            if (!validPassword)
            {
                _logger.LogWarning("Customer Login failed due to invalid credentials.");

                throw new UnauthorizedAccessException("Invalid Email or Password.");
            }

            // Prevent inactive Customers from logging in.
            if (!customer.IsActive)
            {
                throw new BusinessException("Customer account is inactive.");
            }

            // Both Email and Mobile must be verified before Login.
            if (!customer.IsEmailConfirmed || !customer.IsMobileConfirmed)
            {
                throw new BusinessException(
                    "Please verify your Email Address and " +
                    "Mobile Number before Login.");
            }

            // If 2FA is enabled, generate and send a Login verification code.
            if (customer.IsTwoFactorEnabled)
            {
                var code =
                    await CreateVerificationCodeAsync(
                        VerificationPurpose.TwoFactorLogin,
                        customer.Email,
                        customer.Id,
                        _verificationOptions.CodeExpiryMinutes);

                var body = $"""
                    <h3>Two-Factor Authentication</h3>

                    <p>Hello {customer.FirstName},</p>

                    <p>Your Login verification code is:</p>

                    <h2>{code}</h2>

                    <p>
                        This code is valid for
                        {_verificationOptions.CodeExpiryMinutes} minutes.
                    </p>
                    """;

                // Send the 2FA code through Email.
                await _emailService.SendEmailAsync(customer.Email, "Login Verification Code", body);

                _logger.LogInformation(
                    "Two-Factor Authentication is required for Customer {CustomerId}.",
                    customer.Id);

                // Do not generate tokens yet.
                // Tokens will be generated only after successful 2FA verification.
                return new LoginResponseDTO
                {
                    RequiresTwoFactor = true,
                    Customer = customer.ToResponseDTO(),
                    Tokens = null
                };
            }

            // Generate Access and Refresh Tokens when 2FA is not required.
            var tokens = await _tokenService.GenerateTokensAsync(customer);

            _logger.LogInformation("Customer {CustomerId} logged in successfully.", customer.Id);

            // Return Customer details and generated tokens.
            return new LoginResponseDTO
            {
                RequiresTwoFactor = false,
                Customer = customer.ToResponseDTO(),
                Tokens = tokens
            };
        }

        public async Task<TokenResponseDTO> VerifyTwoFactorAsync(TwoFactorLoginVerificationRequestDTO request)
        {
            // Validate the submitted 2FA verification code.
            var verificationCode =
                await ValidateVerificationCodeAsync(
                    VerificationPurpose.TwoFactorLogin,
                    request.Email,
                    request.Code,
                    _verificationOptions.MaxFailedAttempts);

            // Load the Customer associated with the verification code.
            var customer = await _customerRepository.GetByIdAsync(verificationCode.CustomerId);

            // The Customer must exist and be active.
            if (customer == null || !customer.IsActive)
            {
                throw new UnauthorizedAccessException("Customer account is not available.");
            }

            // Ensure 2FA is still enabled.
            if (!customer.IsTwoFactorEnabled)
            {
                throw new BusinessException("Two-Factor Authentication is not enabled.");
            }

            _logger.LogInformation(
                "Two-Factor Authentication completed successfully for Customer {CustomerId}.",
                customer.Id);

            // Generate Access and Refresh Tokens after successful 2FA.
            return await _tokenService.GenerateTokensAsync(customer);
        }

        public Task<TokenResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO request)
        {
            _logger.LogInformation("Refresh Token request received.");

            // Delegate Refresh Token rotation to TokenService.
            return _tokenService.RefreshTokensAsync(request.RefreshToken);
        }

        public Task LogoutAsync(LogoutRequestDTO request)
        {
            _logger.LogInformation("Customer Logout request received.");

            // Revoke the current Refresh Token.
            return _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        }

        public async Task EnableTwoFactorAsync(int customerId)
        {
            // Load the Customer as a tracked Entity.
            var customer = await _customerRepository.GetByIdForUpdateAsync(customerId);

            if (customer == null || !customer.IsActive)
            {
                throw new NotFoundException("Customer was not found or not active.");
            }

            // Email and Mobile must be verified before enabling 2FA.
            if (!customer.IsEmailConfirmed || !customer.IsMobileConfirmed)
            {
                throw new BusinessException(
                    "Email Address and Mobile Number must " +
                    "be verified before enabling 2FA.");
            }

            // Enable Two-Factor Authentication.
            customer.IsTwoFactorEnabled = true;
            customer.UpdatedAtUtc = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Two-Factor Authentication enabled for Customer {CustomerId}.",
                customerId);
        }

        public async Task DisableTwoFactorAsync(int customerId)
        {
            // Load the Customer as a tracked Entity.
            var customer = await _customerRepository.GetByIdForUpdateAsync(customerId);

            if (customer == null || !customer.IsActive)
            {
                throw new NotFoundException("Customer was not found or not active.");
            }

            // Disable Two-Factor Authentication.
            customer.IsTwoFactorEnabled = false;
            customer.UpdatedAtUtc = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Two-Factor Authentication disabled for Customer {CustomerId}.",
                customerId);
        }

        #region Private Helper methods

        private async Task<string> CreateVerificationCodeAsync(
            VerificationPurpose purpose,
            string identifier,
            int customerId,
            int expiryMinutes)
        {
            // Generate a new verification code.
            var code = VerificationCodeHelper.GenerateCode();

            // Create the VerificationCode Entity.
            var verificationCode = new VerificationCode
            {
                CustomerId = customerId,

                // Indicates why the code was created:
                // Email verification, Mobile verification, or 2FA.
                Purpose = purpose,

                // Store the normalized Email or Mobile Number.
                Identifier = NormalizeIdentifier(identifier),

                // Define when the code expires.
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes),

                // Start with zero failed attempts.
                FailedAttempts = 0,

                // Null means the code has not been used yet.
                UsedAtUtc = null
            };

            // Hash the verification code before storing it.
            // The plain code is only returned for Email/SMS delivery.
            verificationCode.CodeHash = VerificationCodeHelper.HashCode(verificationCode, code);

            // Add the verification code to the database.
            await _verificationCodeRepository.AddAsync(verificationCode);

            // Save the verification code.
            await _unitOfWork.SaveChangesAsync();

            // Return the plain code so that it can be
            // sent through Email or SMS.
            return code;
        }

        private async Task<VerificationCode> ValidateVerificationCodeAsync(
            VerificationPurpose purpose, //2FA, Email verification, mobile number verification
            string identifier, //Email or Mobile Number
            string code, //Authentication code, 6 digit integer
            int maxAttempts) //
        {
            // Retrieve the latest verification code for the given purpose and identifier.
            var verificationCode =
                await _verificationCodeRepository
                    .GetLatestForUpdateAsync(purpose, NormalizeIdentifier(identifier));

            // Reject if:
            // 1. No code exists
            // 2. Code has already been used
            // 3. Code has expired
            if (verificationCode == null ||
                verificationCode.UsedAtUtc.HasValue ||
                verificationCode.ExpiresAtUtc <= DateTime.UtcNow)
            {
                throw new BusinessException("Verification Code is invalid or expired.");
            }

            // Reject if the maximum number of failed attempts
            // has already been reached.
            if (verificationCode.FailedAttempts >= maxAttempts)
            {
                throw new BusinessException("Maximum verification attempts exceeded.");
            }

            // Compare the submitted code with the stored hash.
            var isValid = VerificationCodeHelper.VerifyCode(
                    verificationCode,
                    verificationCode.CodeHash,
                    code);

            if (!isValid)
            {
                // Increase failed attempt count.
                verificationCode.FailedAttempts++;

                verificationCode.UpdatedAtUtc = DateTime.UtcNow;

                // Save the failed attempt immediately.
                await _unitOfWork.SaveChangesAsync();

                // If the maximum failed attempts are now reached,
                // reject further verification attempts.
                if (verificationCode.FailedAttempts >= maxAttempts)
                {
                    throw new BusinessException("Maximum verification attempts exceeded.");
                }

                throw new BusinessException("Invalid Verification Code.");
            }

            // Mark the code as used after successful validation.
            verificationCode.UsedAtUtc = DateTime.UtcNow;
            verificationCode.UpdatedAtUtc = DateTime.UtcNow;

            // Save the failed attempt immediately.
            await _unitOfWork.SaveChangesAsync();

            // The caller will save this change together with
            // the related Customer update.
            return verificationCode;
        }

        private static string NormalizeIdentifier(string identifier)
        {
            // Trim unnecessary spaces and make the value lowercase.
            // This is mainly useful for Email identifiers.
            return identifier.Trim().ToLowerInvariant();
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            // Remove leading and trailing spaces.
            var value = phoneNumber.Trim();

            // Ensure the phone number starts with "+"
            // so it follows the expected international format.
            if (!value.StartsWith("+"))
            {
                value = "+" + value;
            }

            return value;
        }

        #endregion
    }
}
