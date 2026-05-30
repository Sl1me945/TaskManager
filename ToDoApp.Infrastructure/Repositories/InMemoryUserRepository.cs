using ToDoApp.Domain.Entities;
using ToDoApp.Application.Interfaces;

namespace ToDoApp.Infrastructure.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new List<User>();
        public InMemoryUserRepository() { }

        public Task<IReadOnlyList<User>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<User>>(_users.AsReadOnly());
        }
        public Task<User?> GetByIdAsync(Guid id)
        {
            User? user = _users.FirstOrDefault(u => u.Id == id);

            return Task.FromResult(user);
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

            existingUser.UpdateUsername(user.Username);
            existingUser.UpdatePasswordHash(user.PasswordHash);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == id)
                ?? throw new InvalidOperationException($"User with id {id} not found");

            _users.Remove(existingUser);

            return Task.CompletedTask;
        }
        public Task<int> SaveChangesAsync()
        {
            return Task.FromResult(1);
        }
    }
}