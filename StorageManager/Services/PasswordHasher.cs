using System;
using System.Security.Cryptography;

namespace StorageManager.Services
{
    public static class PasswordHasher
    {
        // PBKDF2
        public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                salt = new byte[16];
                rng.GetBytes(salt);
            }

            using (var derive = new Rfc2898DeriveBytes(password, salt, 100_000))
            {
                hash = derive.GetBytes(32);
            }
        }

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var derive = new Rfc2898DeriveBytes(password, storedSalt, 100_000))
            {
                var computed = derive.GetBytes(32);
                if (computed.Length != storedHash.Length) return false;
                for (int i = 0; i < computed.Length; i++)
                {
                    if (computed[i] != storedHash[i]) return false;
                }
                return true;
            }
        }
    }
}