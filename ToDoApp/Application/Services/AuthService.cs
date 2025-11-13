using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities;

namespace ToDoApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthService(ILogger<AuthService> logger, IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _logger = logger;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task SignUpAsync(string username, string password)
        {
            _logger.LogInformation("Sign up attempt for user: {username}", username);
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
                throw new InvalidOperationException("This username is already taken.");

            string passwordHash = _passwordHasher.Hash(password);
            var user = new User(username, passwordHash);

            _logger.LogInformation("Succesfully sign up for user: {username}", username);
            await _userRepository.AddAsync(user);
        }

        public async Task<string?> SignInAsync(string username, string password)
        {
            _logger.LogInformation("Sign in attempt for user: {username}", username);
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || !_passwordHasher.Verify(user.PasswordHash, password))
                return null;

            var tokenString = _tokenService.GenerateToken(user);

            return tokenString;
        }

        public async Task SignOutAsync(string? token)
        {
            _logger.LogInformation("Sign out");
            // Revoke token if provided
            if (!string.IsNullOrWhiteSpace(token))
            {
                await _tokenService.RevokeTokenAsync(token);
            }
        }
    }
}
