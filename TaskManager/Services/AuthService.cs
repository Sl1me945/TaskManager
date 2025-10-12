using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Interfaces;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        public User? CurrentUser { get; private set; }

        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task SignUpAsync(string username, string password, bool? isAdmin = null)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
                throw new InvalidOperationException("This username is already taken.");

            string passwordHash = _passwordHasher.Hash(password);
            var user = new User(username, passwordHash, isAdmin ?? false);

            await _userRepository.AddAsync(user);
        }

        public async Task<bool> SignInAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                return false;

            if (!_passwordHasher.Verify(user.PasswordHash, password))
                return false;

            CurrentUser = user;
            return true;
        }

        public void SignOut()
        {
            CurrentUser = null;
        }
    }

}
