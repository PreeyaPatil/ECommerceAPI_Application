using ECommerceAPI.Data;
using ECommerceAPI.Entities;
using ECommerceAPI.Enums;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Repositories.Implementations
{
    public sealed class VerificationCodeRepository : IVerificationCodeRepository
    {
        private readonly ECommerceDbContext _context;

        // Initializes the repository with the application's database context.
        public VerificationCodeRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        // Gets the latest verification code for the given purpose and identifier
        // with tracking enabled so that it can be updated.
        public async Task<VerificationCode?> GetLatestForUpdateAsync(
            VerificationPurpose purpose,
            string identifier)
        {
            return await _context.VerificationCodes
                .Where(x => x.Purpose == purpose && x.Identifier == identifier)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        // Adds a new verification code to the DbContext for insertion into the database.
        public async Task AddAsync(VerificationCode verificationCode)
        {
            await _context.VerificationCodes
                .AddAsync(verificationCode);
        }
    }
}

