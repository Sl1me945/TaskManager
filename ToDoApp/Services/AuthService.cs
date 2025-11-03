using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoApp.Interfaces;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        public User? CurrentUser { get; private set; }

        public AuthService(ILogger logger, IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _logger = logger;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
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

        public async Task<bool> SignInAsync(string username, string password)
        {
            _logger.Info($"Sign in attempt for user: {username}");
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                return false;

            if (!_passwordHasher.Verify(user.PasswordHash, password))
                return false;

            CurrentUser = user;
            _logger.Info($"Succesfully sign in for user: {username}");
            return true;
        }

        public void SignOut()
        {
            _logger.Info("Sign out");
            CurrentUser = null;
        }
    }

}
