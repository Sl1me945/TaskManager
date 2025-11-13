using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities.Tasks;

namespace ToDoApp.Application.Services
{
    public class TaskManager(ILogger<TaskManager> logger, ITaskRepository taskRepository, IUserRepository userRepository) : ITaskManager
    {
        private readonly ITaskRepository _taskRepository = taskRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ILogger<TaskManager> _logger = logger;

        public async Task AddTaskAsync(Guid userId, BaseTask task)
        {
            _ = await _userRepository.GetByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ArgumentException("Task title cannot be empty.");

            task.UserId = userId;
            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} added for user {UserId}", task.Id, userId);
        }

        public async Task UpdateTaskAsync(Guid userId, BaseTask task)
        {
            if (task.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own tasks.");

            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ArgumentException("Task title cannot be empty.");

            await _taskRepository.UpdateAsync(task);
            await _taskRepository.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} updated for user {UserId}", task.Id, userId);
        }

        public async Task RemoveTaskAsync(Guid userId, Guid taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId)
                ?? throw new InvalidOperationException($"Task {taskId} not found.");

            if (task.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own tasks.");

            await _taskRepository.DeleteAsync(taskId);
            await _taskRepository.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} removed for user {UserId}", taskId, userId);
        }

        public async Task MarkAsCompletedAsync(Guid userId, Guid taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId)
                ?? throw new InvalidOperationException($"Task {taskId} not found.");

            if (task.UserId != userId)
                throw new UnauthorizedAccessException("You can only modify your own tasks.");

            task.MarkAsCompleted();
            await _taskRepository.UpdateAsync(task);
            await _taskRepository.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} marked as completed for user {UserId}", taskId, userId);
        }

        public async Task<IEnumerable<BaseTask>> SearchAsync(Guid userId, string keyword)
        {
            var tasks = await _taskRepository.GetByUserIdAsync(userId);
            return tasks.Where(t => t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                 || (t.Description?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        public async Task<IEnumerable<BaseTask>> SortByDateAsync(Guid userId, bool ascending = true)
        {
            var tasks = await _taskRepository.GetByUserIdAsync(userId);
            return ascending ? tasks.OrderBy(t => t.DueDate) : tasks.OrderByDescending(t => t.DueDate);
        }

        public async Task<IEnumerable<BaseTask>> FilterByCompletionAsync(Guid userId, bool isCompleted)
        {
            var tasks = await _taskRepository.GetByUserIdAsync(userId);
            return tasks.Where(t => t.IsCompleted == isCompleted);
        }
    }
}