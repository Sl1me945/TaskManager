using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using ToDoApp.Application.DTOs;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities.Tasks;
using ToDoApp.Domain.Enums;
using ToDoApp.Infrastructure.Services;

namespace ToDoApp.Infrastructure.Repositories
{
    public class FileTaskRepository : ITaskRepository
    {
        private readonly ILogger<FileTaskRepository> _logger;
        private readonly IFileStorage _fileStorage;
        private readonly string _filePath;

        public FileTaskRepository(ILogger<FileTaskRepository> logger, IFileStorage fileStorage, string filePath)
        {
            _logger = logger;
            _fileStorage = fileStorage;
            _filePath = filePath;
        }

        public async Task<IReadOnlyList<BaseTask>> GetAllAsync()
        {
            var tasks = await LoadAllTasksAsync();
            IReadOnlyList<BaseTask> tasksReadOnly = tasks.AsReadOnly();

            return tasksReadOnly;
        }
        public async Task<IReadOnlyList<BaseTask>> GetByUserIdAsync(Guid userId)
        {
            var tasks = await LoadAllTasksAsync();
            IReadOnlyList<BaseTask> tasksByUserId = tasks.Where(t => t.UserId == userId).ToList().AsReadOnly();

            return tasksByUserId;
        }
        public async Task<BaseTask?> GetByIdAsync(Guid id)
        {
            var tasks = await LoadAllTasksAsync();
            BaseTask? task = tasks.FirstOrDefault(t => t.Id == id);

            return task;
        }
        public async Task AddAsync(BaseTask task)
        {
            var tasks = await LoadAllTasksAsync();

            tasks.Add(task);

            await SaveAllTasksAsync(tasks);
        }
        public async Task UpdateAsync(BaseTask task)
        {
            var tasks = await LoadAllTasksAsync();

            var index = tasks.FindIndex(t => t.Id == task.Id);
            if (index == -1)
                throw new InvalidOperationException($"Task with title {task.Title} not found");

            tasks[index] = task;

            await SaveAllTasksAsync(tasks);
        }
        public async Task DeleteAsync(Guid id)
        {
            var tasks = await LoadAllTasksAsync();

            var existingTask = tasks.FirstOrDefault(t => t.Id == id)
               ?? throw new InvalidOperationException($"Task with id {id} not found");

            tasks.Remove(existingTask);

            await SaveAllTasksAsync(tasks);
        }
        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }

        // Mapping functions
        private static BaseTask MapToDomain(TaskDto taskDto)
        {
            BaseTask task = taskDto.Type switch
            {
                TaskType.Simple => new SimpleTask(taskDto.Id, taskDto.CreatedAt),
                TaskType.Work => new WorkTask(taskDto.Id, taskDto.CreatedAt) { ProjectName = taskDto.ProjectName ?? "" },
                TaskType.Recurring => new RecurringTask(taskDto.Id, taskDto.CreatedAt) { RepeatInterval = taskDto.RepeatInterval ?? TimeSpan.Zero },
                _ => new SimpleTask(taskDto.Id, taskDto.CreatedAt)
            };
            task.UserId = taskDto.UserId;
            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.DueDate = taskDto.DueDate;
            if (taskDto.IsCompleted) task.MarkAsCompleted();
            task.Priority = taskDto.Priority;

            return task;
        }
        private static TaskDto MapToDto(BaseTask task)
        {
            var taskDto = new TaskDto
            {
                Id = task.Id,
                UserId = task.UserId,
                Title = task.Title,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                Priority = task.Priority,
                Type = task switch
                {
                    WorkTask => TaskType.Work,
                    RecurringTask => TaskType.Recurring,
                    _ => TaskType.Simple
                }
            };
            if (task is WorkTask w) taskDto.ProjectName = w.ProjectName;
            if (task is RecurringTask r) taskDto.RepeatInterval = r.RepeatInterval;
            return taskDto;
        }

        // Other functions
        private async Task<List<BaseTask>> LoadAllTasksAsync()
        {
            try
            {
                var dtos = await _fileStorage.LoadAsync<List<TaskDto>>(_filePath);

                return (dtos ?? [])
                    .Select(MapToDomain)
                    .ToList();
            }
            catch (FileNotFoundException) 
            {
                _logger.LogWarning("Task storage file not found. Creating new empty storage.");
                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load tasks from file: {FilePath}", _filePath);
                return [];
            }
        }
        private async Task SaveAllTasksAsync(IEnumerable<BaseTask> tasks)
        {
            var dtos = tasks.Select(MapToDto).ToList();
            await _fileStorage.SaveAsync(_filePath, dtos);
        }
    }
}