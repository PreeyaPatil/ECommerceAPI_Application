using System.Security.Cryptography;
using System.Text;

namespace ECommerceAPI.Helpers
{
    public static class TokenHashHelper
    {
        // Converts the given token into a SHA-256 hash.
        // Example:
        // Original Token: abc123xyz
        // Stored Value: 7F83B1657FF1FC53...
        public static string HashToken(string token)
        {
            // Convert the token string into a byte array
            // because SHA-256 works with binary data.
            var tokenBytes = Encoding.UTF8.GetBytes(token);

            // Generate a SHA-256 hash from the token bytes.
            // SHA-256 always produces a 256-bit (32-byte) hash value.
            var hashBytes = SHA256.HashData(tokenBytes);

            // Convert the hash bytes into a readable hexadecimal string
            // so that it can be easily stored in the database.
            return Convert.ToHexString(hashBytes);
        }
    }
}
