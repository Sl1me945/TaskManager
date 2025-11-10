using ToDoApp.Domain.Entities;

namespace ToDoApp.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<User>> GetAllAsync();
        Task<User?> GetByUsernameAsync(string username);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task SaveChangeAsync();
    }
}
