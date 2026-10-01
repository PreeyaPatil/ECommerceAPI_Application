using ECommerceAPI.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace ECommerceAPI.Helpers
{
    public static class VerificationCodeHelper
    {
        // Generates a secure random 6-digit verification code.
        // RandomNumberGenerator.GetInt32 uses a cryptographically secure
        // random number generator, which is more suitable for OTPs
        // and verification codes.

        // The minimum value is inclusive and the maximum value is exclusive.
        // So this generates values from:
        // 100000 to 999999
        // Example:
        // 483721
        public static string GenerateCode()
        {
            return RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();
        }

        // Converts the plain verification code into a secure hash
        // before storing it in the database.
        // Example:
        // Plain Code: 483721
        // Stored Value: AQAAAAIAAYagAAAAE...
        // We should store the hash instead of storing
        // the original verification code in plain text.
        public static string HashCode(VerificationCode verificationCode, string code)
        {
            // Create ASP.NET Core Identity's password hasher.
            // Although this class is commonly used for passwords,
            // it can also securely hash values such as verification codes.
            var passwordHasher = new PasswordHasher<VerificationCode>();

            // Hash the verification code and return the generated hash.
            // The VerificationCode entity is also supplied as context to the PasswordHasher.
            return passwordHasher.HashPassword(verificationCode, code);
        }

        // Verifies whether the verification code entered by the user
        // matches the previously stored hash.
        public static bool VerifyCode(VerificationCode verificationCode, string codeHash, string code)
        {
            // Create the same ASP.NET Core Identity password hasher
            // that is used for hashing the verification code.
            var passwordHasher = new PasswordHasher<VerificationCode>();

            // Compare the plain verification code entered by the user
            // with the hashed verification code stored in the database.
            var result = passwordHasher.VerifyHashedPassword(verificationCode, codeHash, code);

            // VerifyHashedPassword can return:
            // Failed
            // Success
            return result != PasswordVerificationResult.Failed;
        }
    }
}
