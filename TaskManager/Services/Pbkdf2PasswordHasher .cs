using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Interfaces;

namespace TaskManager.Services
{
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public string Hash(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool Verify(string hashed, string password)
        {
            var parts = hashed.Split('.');
            if (parts.Length != 3) return false;
            int iter = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var hash = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iter, HashAlgorithmName.SHA256);
            var candidate = pbkdf2.GetBytes(hash.Length);
            return CryptographicOperations.FixedTimeEquals(candidate, hash);
        }
    }
}
