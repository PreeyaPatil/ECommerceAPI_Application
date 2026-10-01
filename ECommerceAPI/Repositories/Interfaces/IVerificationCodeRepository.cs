using ECommerceAPI.Entities;
using ECommerceAPI.Enums;

namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IVerificationCodeRepository
    {
        // Gets the latest verification code for the given purpose
        // Identifier is typically used to store the value for which
        // the verification code was generated.
        // Identifier = "pranaya@example.com";
        // Identifier = "+919876543210";
        Task<VerificationCode?> GetLatestForUpdateAsync(
            VerificationPurpose purpose,
            string identifier);

        // Adds a new verification code to the database context.
        Task AddAsync(VerificationCode verificationCode);
    }
}
