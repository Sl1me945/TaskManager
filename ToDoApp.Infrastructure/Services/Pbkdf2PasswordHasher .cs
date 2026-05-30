using System.Security.Cryptography;
using ToDoApp.Application.Interfaces;

namespace ToDoApp.Infrastructure.Services
{
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password must not be empty", nameof(password));

            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            byte[] hash = Array.Empty<byte>();
            try
            {
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
                hash = pbkdf2.GetBytes(HashSize);

                // Convert to base64 before wiping sensitive buffers
                var saltB64 = Convert.ToBase64String(salt);
                var hashB64 = Convert.ToBase64String(hash);
                return $"{Iterations}.{saltB64}.{hashB64}";
            }
            finally
            {
                // Zero sensitive data in memory
                if (hash?.Length > 0)
                    CryptographicOperations.ZeroMemory(hash);
                CryptographicOperations.ZeroMemory(salt);
            }
        }

        public bool Verify(string hashed, string password)
        {
            if (string.IsNullOrWhiteSpace(hashed) || string.IsNullOrWhiteSpace(password))
                return false;

            var parts = hashed.Split('.');
            if (parts.Length != 3) return false;

            if (!int.TryParse(parts[0], out var iter) || iter <= 0) return false;

            byte[] salt;
            byte[] hash;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                hash = Convert.FromBase64String(parts[2]);
            }
            catch
            {
                return false;
            }

            byte[] candidate = Array.Empty<byte>();
            try
            {
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iter, HashAlgorithmName.SHA256);
                candidate = pbkdf2.GetBytes(hash.Length);

                // Constant-time comparison
                return CryptographicOperations.FixedTimeEquals(candidate, hash);
            }
            finally
            {
                if (candidate?.Length > 0)
                    CryptographicOperations.ZeroMemory(candidate);
                if (hash?.Length > 0)
                    CryptographicOperations.ZeroMemory(hash);
                CryptographicOperations.ZeroMemory(salt);
            }
        }
    }
}
