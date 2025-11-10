using ToDoApp.Domain.Entities.Tasks;

namespace ToDoApp.Application.Interfaces
{
    public interface ITaskManager
    {
        Task AddTaskAsync(string token, BaseTask task);
        Task RemoveTaskAsync(string token, Guid taskId);
        Task MarkAsCompletedAsync(string token, Guid taskId);
        Task<IEnumerable<BaseTask>> SearchAsync(string token, string keyword);
        Task<IEnumerable<BaseTask>> SortByDateAsync(string token, bool ascending = true);
        Task<IEnumerable<BaseTask>> FilterByCompletionAsync(string token, bool isCompleted);
    }
}
