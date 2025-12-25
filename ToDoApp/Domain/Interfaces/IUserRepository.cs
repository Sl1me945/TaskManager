using ToDoApp.Domain.Entities;

namespace ToDoApp.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByUsernameAsync(string username);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task SaveChangesAsync();
    }
}
