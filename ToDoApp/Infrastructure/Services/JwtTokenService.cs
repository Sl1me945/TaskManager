using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities;

namespace ToDoApp.Infrastructure.Services
{
    public class JwtTokenService : ITokenService
    {
        private const string SecretKey = "super_puper_secret_key_kkkkkkkkkkkkk";
        private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(SecretKey));

        // revoked JTI -> expiry (UTC)
        private readonly ConcurrentDictionary<string, DateTime> _revoked = new();

        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = creds,
                Issuer = "ToDoApp",
                Audience = "ToDoAppClient"
            };

            var tokenHandler = new JsonWebTokenHandler();
            var tokenString = tokenHandler.CreateToken(token);

            return tokenString;
        }

        public async Task<TokenValidationResult> ValidateTokenAsync(string token)
        {
            var tokenHandler = new JsonWebTokenHandler();

            var validationResult = await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "ToDoApp",
                ValidAudience = "ToDoAppClient",
                IssuerSigningKey = _key,
            });
            
            // If token structurally validated, check revocation by jti
            if (validationResult.IsValid && validationResult.ClaimsIdentity != null)
            {
                var jti = validationResult.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (!string.IsNullOrEmpty(jti))
                {
                    if (_revoked.TryGetValue(jti, out var expiryUtc))
                    {
                        // If token jti is found in revoked list and not yet expired, treat as invalid
                        if (expiryUtc > DateTime.UtcNow)
                        {
                            return new TokenValidationResult
                            {
                                IsValid = false,
                                Exception = new SecurityTokenException("Token revoked")
                            };
                        }
                        else
                        {
                            // cleanup stale entry
                            _revoked.TryRemove(jti, out _);
                        }
                    }
                }
            }

            return validationResult;
        }

        public Task RevokeTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Task.CompletedTask;

            // try to parse payload and extract jti and exp
            if (TryExtractJtiAndExp(token, out var jti, out var expUtc) && !string.IsNullOrEmpty(jti))
            {
                var expiry = expUtc ?? DateTime.UtcNow.AddHours(1);
                _revoked[jti] = expiry;
            }

            return Task.CompletedTask;
        }

        // helper: parse JWT payload (base64url) to extract jti and exp (exp expected as numeric unix epoch)
        private static bool TryExtractJtiAndExp(string token, out string? jti, out DateTime? expUtc)
        {
            jti = null;
            expUtc = null;
            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2) return false;

                var payload = parts[1];
                // base64url -> base64
                var padded = payload.Replace('-', '+').Replace('_', '/');
                switch (padded.Length % 4)
                {
                    case 2: padded += "=="; break;
                    case 3: padded += "="; break;
                    case 1: padded += "==="; break;
                }

                var bytes = Convert.FromBase64String(padded);
                using var doc = JsonDocument.Parse(bytes);
                var root = doc.RootElement;

                if (root.TryGetProperty("jti", out var jtiProp) && jtiProp.ValueKind == JsonValueKind.String)
                    jti = jtiProp.GetString();

                if (root.TryGetProperty("exp", out var expProp) && expProp.ValueKind == JsonValueKind.Number)
                {
                    if (expProp.TryGetInt64(out var seconds))
                    {
                        expUtc = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
                    }
                    else if (expProp.TryGetDouble(out var dbl))
                    {
                        var secs = Convert.ToInt64(dbl);
                        expUtc = DateTimeOffset.FromUnixTimeSeconds(secs).UtcDateTime;
                    }
                }

                return !string.IsNullOrEmpty(jti);
            }
            catch
            {
                return false;
            }
        }
    }
}