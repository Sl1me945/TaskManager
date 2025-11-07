using Microsoft.IdentityModel.Tokens;
using ToDoApp.Models;

namespace ToDoApp.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        Task<TokenValidationResult> ValidateTokenAsync(string token);
    }
}
