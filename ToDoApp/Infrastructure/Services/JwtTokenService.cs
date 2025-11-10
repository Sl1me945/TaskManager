using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Text;
using System.Security.Claims;
using ToDoApp.Domain.Entities;
using ToDoApp.Application.Interfaces;

namespace ToDoApp.Infrastructure.Services
{
    public class JwtTokenService : ITokenService
    {
        private const string SecretKey = "super_puper_secret_key_kkkkkkkkkkkkk";
        private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(SecretKey));

        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
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

            return validationResult;
        }
    }
}
