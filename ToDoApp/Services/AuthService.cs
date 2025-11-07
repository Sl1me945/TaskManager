using ToDoApp.Interfaces;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        public User? CurrentUser { get; private set; }

        public AuthService(ILogger logger, IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _logger = logger;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task SignUpAsync(string username, string password)
        {
            _logger.Info($"Sign up attempt for user: {username}");
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
                throw new InvalidOperationException("This username is already taken.");

            string passwordHash = _passwordHasher.Hash(password);
            var user = new User(username, passwordHash);

            _logger.Info($"Succesfully sign up for user: {username}");
            await _userRepository.AddAsync(user);
        }

        public async Task<string?> SignInAsync(string username, string password)
        {
            _logger.Info($"Sign in attempt for user: {username}");
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || !_passwordHasher.Verify(user.PasswordHash, password))
                return null;

            var tokenString = _tokenService.GenerateToken(user);

            return tokenString;
        }

        public void SignOut()
        {
            _logger.Info("Sign out");
            CurrentUser = null;
        }
    }
}
