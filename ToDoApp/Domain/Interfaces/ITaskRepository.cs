using ToDoApp.Domain.Entities.Tasks;

namespace ToDoApp.Domain.Interfaces
{
    public interface ITaskRepository
    {
        Task<IReadOnlyList<BaseTask>> GetAllAsync();
        Task<IReadOnlyList<BaseTask>> GetByUserIdAsync(Guid userId);
        Task<BaseTask?> GetByIdAsync(Guid id);
        Task AddAsync(BaseTask task);
        Task UpdateAsync(BaseTask task);
        Task DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
