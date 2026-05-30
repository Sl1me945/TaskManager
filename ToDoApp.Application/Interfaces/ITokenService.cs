using Microsoft.IdentityModel.Tokens;
using ToDoApp.Domain.Entities;

namespace ToDoApp.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        Task<TokenValidationResult> ValidateTokenAsync(string token);
        Task RevokeTokenAsync(string token);
    }
}
