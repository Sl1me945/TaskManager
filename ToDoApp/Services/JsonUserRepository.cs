using ToDoApp.Interfaces;
using ToDoApp.Models;
using ToDoApp.Models.Tasks;
using ToDoApp.DTOs;

namespace ToDoApp.Services
{
    public class JsonUserRepository : IUserRepository
    {
        private readonly string _jsonFilePath;
        private readonly ILogger _logger;
        private readonly IFileStorage _fileStorageService;

        public JsonUserRepository(ILogger logger, IFileStorage fileStorageService, string jsonFilePath = "data/users/users.json") 
        {
            _logger = logger;
            _fileStorageService = fileStorageService;
            _jsonFilePath = jsonFilePath;

            var directory = Path.GetDirectoryName(_jsonFilePath) ?? ".";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (!File.Exists(_jsonFilePath))
                File.WriteAllText(_jsonFilePath, "[]");
        }

        public async Task<IReadOnlyList<User>> GetAllAsync()
        {
            _logger.Info("Get all users");
            var usersDto = await _fileStorageService.LoadAsync<IReadOnlyList<UserDto>>(_jsonFilePath);
            IReadOnlyList<User> users = usersDto.Select(u => MapToDomain(u)).ToList().AsReadOnly();

            return users;
        }
        public async Task<User?> GetByUsernameAsync(string username)
        {
            _logger.Info($"Get by username: {username}");
            var usersDto = await _fileStorageService.LoadAsync<List<UserDto>>(_jsonFilePath);
            var users = usersDto.Select(u => MapToDomain(u)).ToList();

            return users.FirstOrDefault(x => x.Username == username);
        }
        public async Task AddAsync(User user)
        {
            _logger.Info($"Add user: {user.Username}");
            var usersDto = await _fileStorageService.LoadAsync<List<UserDto>>(_jsonFilePath);
            var users = usersDto.Select(u => MapToDomain(u)).ToList();

            users.Add(user);

            usersDto = users.Select(u => MapToDto(u)).ToList();
            await _fileStorageService.SaveAsync<List<UserDto>>(_jsonFilePath, usersDto);
            return;
        }
        public async Task UpdateAsync(User user)
        {
            _logger.Info($"Update user: {user.Username}");
            var usersDto = await _fileStorageService.LoadAsync<List<UserDto>>(_jsonFilePath);
            var users = usersDto.Select(u => MapToDomain(u)).ToList();
            var existingUser = users.FirstOrDefault(x => x.Username == user.Username);

            if (existingUser is null)
                throw new InvalidOperationException($"User with username {user.Username} not found");

            existingUser.Username = user.Username;
            existingUser.PasswordHash = user.PasswordHash;
            existingUser.Tasks = user.Tasks;

            usersDto = users.Select(u => MapToDto(u)).ToList();
            await _fileStorageService.SaveAsync<List<UserDto>>(_jsonFilePath, usersDto);
            return;
        }
        public Task SaveChangeAsync()
        {
            return Task.CompletedTask;
        }

        // Mapping functions
        private static User MapToDomain(UserDto dto)
        {
            var u = new User() { Username = dto.Username, PasswordHash = dto.PasswordHash};
            foreach (var t in dto.Tasks)
            {
                ITask? task = t.Type switch
                {
                    TaskType.Simple => new SimpleTask(t.Id, t.CreatedAt),
                    TaskType.Work => new WorkTask(t.Id, t.CreatedAt) { ProjectName = t.ProjectName ?? "" },
                    TaskType.Recurring => new RecurringTask(t.Id, t.CreatedAt) { RepeatInterval = t.RepeatInterval ?? TimeSpan.Zero },
                    _ => new SimpleTask(t.Id, t.CreatedAt)
                };
                task.Title = t.Title;
                task.Description = t.Description;
                task.DueDate = t.DueDate;
                if(t.IsCompleted) task.MarkAsCompleted();
                task.Priority = t.Priority;
                u.Tasks.Add(task);
            }
            return u;
        }

        private static UserDto MapToDto(User user)
        {
            var dto = new UserDto { Username = user.Username, PasswordHash = user.PasswordHash};
            dto.Tasks = user.Tasks.Select(t =>
            {
                var tt = new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    Priority = t.Priority,
                    Type = t switch
                    {
                        WorkTask => TaskType.Work,
                        RecurringTask => TaskType.Recurring,
                        _ => TaskType.Simple
                    }
                };
                if (t is WorkTask w) tt.ProjectName = w.ProjectName;
                if (t is RecurringTask r) tt.RepeatInterval = r.RepeatInterval;
                return tt;
            }).ToList();
            return dto;
        }
    }
}
