using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.Security
{
    using System.Security.Cryptography;

    public static class PasswordHasher
    {
        public static string Hash(string password, int iterations = 100_000)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string stored)
        {
            var parts = stored.Split('.');
            if (parts.Length != 3) return false;

            if (!int.TryParse(parts[0], out int iterations)) return false;

            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expectedHash = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] actualHash = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }

}
