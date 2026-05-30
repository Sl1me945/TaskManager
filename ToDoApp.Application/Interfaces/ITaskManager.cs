using ToDoApp.Domain.Entities.Tasks;

namespace ToDoApp.Application.Interfaces
{
    public interface ITaskManager
    {
        Task AddTaskAsync(Guid userId, BaseTask task);
        Task UpdateTaskAsync(Guid userId, BaseTask task);
        Task RemoveTaskAsync(Guid userId, Guid taskId);
        Task MarkAsCompletedAsync(Guid userId, Guid taskId);
        Task<IEnumerable<BaseTask>> SearchAsync(Guid userId, string keyword);
        Task<IEnumerable<BaseTask>> SortByDateAsync(Guid userId, bool ascending = true);
        Task<IEnumerable<BaseTask>> FilterByCompletionAsync(Guid userId, bool isCompleted);
    }
}
