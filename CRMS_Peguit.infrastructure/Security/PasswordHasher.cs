using BCrypt.Net;
using System;

namespace CRMS_Peguit.infrastructure.Security
{
    // Real authentication uses BCrypt (salted, slow-by-design) instead of the
    // plain SHA-256 in NEXA.Model.User, which stays only as an OOP/encapsulation
    // demonstration and is not used for real login.
    //
    // NuGet: Install-Package BCrypt.Net-Next
    public static class PasswordHasher
    {
        public static string Hash(string plainTextPassword)
        {
            if (string.IsNullOrEmpty(plainTextPassword))
                throw new ArgumentException("Password cannot be empty.", nameof(plainTextPassword));

            return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, workFactor: 12);
        }

        public static bool Verify(string plainTextPassword, string? storedHash)
        {
            if (string.IsNullOrEmpty(plainTextPassword) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            var trimmedHash = storedHash.Trim();

            try
            {
                // Check if the hash starts with valid BCrypt identifiers: $2a$, $2b$, $2y$, $2x$
                if (trimmedHash.Length >= 4 &&
                    trimmedHash[0] == '$' &&
                    trimmedHash[1] == '2' &&
                    (trimmedHash[2] == 'a' || trimmedHash[2] == 'b' || trimmedHash[2] == 'y' || trimmedHash[2] == 'x') &&
                    trimmedHash[3] == '$')
                {
                    return BCrypt.Net.BCrypt.Verify(plainTextPassword, trimmedHash);
                }

                // If storedHash is not a BCrypt hash, check for direct equality (e.g. plain text in dev/seed)
                return plainTextPassword == trimmedHash;
            }
            catch (SaltParseException)
            {
                // Graceful fallback for non-BCrypt format or legacy plain text
                return plainTextPassword == trimmedHash;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}