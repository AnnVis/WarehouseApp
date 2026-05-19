using System;
using System.Security.Cryptography;

namespace NetFrameworkDemo.Helpers
{
    public static class PasswordHasher
    {
        // PBKDF2 - create/verify helper (returns salt+iterations+hash base64)
        private const int SaltBytes = 24;
        private const int HashBytes = 24;
        private const int Iterations = 10000;

        public static string CreateHash(string password)
        {
            var salt = new byte[SaltBytes];
            using (var csprng = new RNGCryptoServiceProvider())
            {
                csprng.GetBytes(salt);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            var hash = pbkdf2.GetBytes(HashBytes);

            var saltBase64 = Convert.ToBase64String(salt);
            var hashBase64 = Convert.ToBase64String(hash);
            return $"{Iterations}:{saltBase64}:{hashBase64}";
        }

        public static bool VerifyHash(string password, string storedHash)
        {
            try
            {
                var parts = storedHash.Split(':');
                var iterations = int.Parse(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                var hash = Convert.FromBase64String(parts[2]);

                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
                var testHash = pbkdf2.GetBytes(hash.Length);

                // constant time comparison
                var diff = hash.Length ^ testHash.Length;
                for (var i = 0; i < hash.Length && i < testHash.Length; i++)
                {
                    diff |= hash[i] ^ testHash[i];
                }
                return diff == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}