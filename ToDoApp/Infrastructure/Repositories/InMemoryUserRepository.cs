using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities;

namespace ToDoApp.Infrastructure.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = [];
        public InMemoryUserRepository() { }

        public Task<IReadOnlyList<User>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<User>>(_users.AsReadOnly());
        }
        public Task<User?> GetByUsernameAsync(string username)
        {
            User? user = _users.FirstOrDefault(u => u.Username == username);
            return Task.FromResult(user);
        }
        public Task AddAsync(User user)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }
        public Task UpdateAsync(User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Username == user.Username)
                ?? throw new InvalidOperationException($"User with username {user.Username} not found");
            existingUser.Username = user.Username;
            existingUser.PasswordHash = user.PasswordHash;
            existingUser.Tasks = user.Tasks;

            return Task.CompletedTask;
        }
        public Task SaveChangeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
