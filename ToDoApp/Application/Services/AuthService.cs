using Microsoft.Extensions.Logging;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities;

namespace ToDoApp.Application.Services
{
    public class AuthService(ILogger<AuthService> logger, IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService) : IAuthService
    {
        private readonly ILogger<AuthService> _logger = logger;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly ITokenService _tokenService = tokenService;
        public User? CurrentUser { get; private set; }

        public async Task SignUpAsync(string username, string password)
        {
            _logger.LogInformation($"Sign up attempt for user: {username}");
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
                throw new InvalidOperationException("This username is already taken.");

            string passwordHash = _passwordHasher.Hash(password);
            var user = new User(username, passwordHash);

            _logger.LogInformation($"Succesfully sign up for user: {username}");
            await _userRepository.AddAsync(user);
        }

        public async Task<string?> SignInAsync(string username, string password)
        {
            _logger.LogInformation($"Sign in attempt for user: {username}");
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || !_passwordHasher.Verify(user.PasswordHash, password))
                return null;

            var tokenString = _tokenService.GenerateToken(user);

            return tokenString;
        }

        public void SignOut()
        {
            _logger.LogInformation("Sign out");
            CurrentUser = null;
        }
    }
}
