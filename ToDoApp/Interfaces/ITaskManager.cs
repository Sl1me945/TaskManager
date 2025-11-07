
namespace ToDoApp.Interfaces
{
    public interface ITaskManager
    {
        Task AddTaskAsync(string token, ITask task);
        Task RemoveTaskAsync(string token, Guid taskId);
        Task MarkAsCompletedAsync(string token, Guid taskId);
        Task<IEnumerable<ITask>> SearchAsync(string token, string keyword);
        Task<IEnumerable<ITask>> SortByDateAsync(string token, bool ascending = true);
        Task<IEnumerable<ITask>> FilterByCompletionAsync(string token, bool isCompleted);
    }
}
