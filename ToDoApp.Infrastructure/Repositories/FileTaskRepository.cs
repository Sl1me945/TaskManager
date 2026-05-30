using Microsoft.Extensions.Logging;
using ToDoApp.Application.DTOs;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities.Tasks;
using ToDoApp.Domain.Enums;

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
        public Task<int> SaveChangesAsync()
        {
            return Task.FromResult(1+1);
        }

        // Mapping functions

        private static BaseTask MapToDomain(FileTaskDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException($"Corrupted data: Task {dto.Id} has null Title");

            BaseTask task = dto.Type switch
            {
                TaskType.Simple => new SimpleTask(
                    dto.Id,
                    dto.CreatedAt,
                    dto.Title,
                    dto.Description ?? string.Empty,
                    dto.DueDate,
                    dto.Priority,
                    dto.IsCompleted,
                    dto.UserId
                ),
                TaskType.Work => new WorkTask(
                    dto.Id,
                    dto.CreatedAt,
                    dto.Title,
                    dto.Description ?? string.Empty,
                    dto.DueDate,
                    dto.Priority,
                    dto.IsCompleted,
                    dto.UserId,
                    dto.ProjectName ?? string.Empty
                ),
                TaskType.Recurring => new RecurringTask(
                    dto.Id,
                    dto.CreatedAt,
                    dto.Title,
                    dto.Description ?? string.Empty,
                    dto.DueDate,
                    dto.Priority,
                    dto.IsCompleted,
                    dto.UserId,
                    dto.RepeatInterval ?? TimeSpan.Zero
                ),
                _ => throw new InvalidOperationException($"Unknown task type: {dto.Type}")
            };

            return task;
        }
        private static FileTaskDto MapToDto(BaseTask task)
        {
            var dto = new FileTaskDto
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

            if (task is WorkTask w)
                dto.ProjectName = w.ProjectName;

            if (task is RecurringTask r)
                dto.RepeatInterval = r.RepeatInterval;

            return dto;
        }

        // Other functions
        private async Task<List<BaseTask>> LoadAllTasksAsync()
        {
            try
            {
                var dtos = await _fileStorage.LoadAsync<List<FileTaskDto>>(_filePath);

                return dtos
                    .Select(MapToDomain)
                    .ToList();
            }
            catch (FileNotFoundException) 
            {
                _logger.LogWarning("Task storage file not found. Creating new empty storage.");
                return new List<BaseTask>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load tasks from file: {FilePath}", _filePath);
                return new List<BaseTask>();
            }
        }
        private async Task SaveAllTasksAsync(IEnumerable<BaseTask> tasks)
        {
            var dtos = tasks.Select(MapToDto).ToList();
            try
            {
                await _fileStorage.SaveAsync(_filePath, dtos);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Failed to save tasks to file: {FilePath}", _filePath);
                throw;
            }
        }
    }
}