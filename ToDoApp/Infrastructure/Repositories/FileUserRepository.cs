using Microsoft.Extensions.Logging;
using ToDoApp.Application.DTOs;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities;

namespace ToDoApp.Infrastructure.Repositories
{
    public class FileUserRepository : IUserRepository
    {
        private readonly ILogger<FileUserRepository> _logger;
        private readonly IFileStorage _fileStorage;
        private readonly string _filePath;

        public FileUserRepository(ILogger<FileUserRepository> logger, IFileStorage fileStorageService, string filePath)
        {
            _logger = logger;
            _fileStorage = fileStorageService;
            _filePath = filePath;
        }

        public async Task<IReadOnlyList<User>> GetAllAsync()
        {
            _logger.LogInformation("Get all");

            var users = await LoadAllUsersAsync();

            IReadOnlyList<User> usersReadOnly = users.AsReadOnly();

            return usersReadOnly;
        }
        public async Task<User?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Get by id: {id}", id);

            var users = await LoadAllUsersAsync();

            User? user = users.FirstOrDefault(u => u.Id == id);

            return user;
        }
        public async Task<User?> GetByUsernameAsync(string username)
        {
            _logger.LogInformation("Get by username: {username}", username);

            var users = await LoadAllUsersAsync();

            User? user = users.FirstOrDefault(u => u.Username == username);

            return user;
        }
        public async Task AddAsync(User user)
        {
            var users = await LoadAllUsersAsync();

            users.Add(user);

            await SaveAllUsersAsync(users);
        }
        public async Task UpdateAsync(User user)
        {
            var users = await LoadAllUsersAsync();

            var index = users.FindIndex(t => t.Id == user.Id);
            if (index == -1)
                throw new InvalidOperationException($"User with username {user.Username} not found");

            users[index] = user;

            await SaveAllUsersAsync(users);
        }
        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }

        // Mapping functions
        private static User MapToDomain(UserDto dto)
        {
            var user = new User() { Id = dto.Id, Username = dto.Username, PasswordHash = dto.PasswordHash };
            return user;
        }
        private static UserDto MapToDto(User user)
        {
            var dto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                PasswordHash = user.PasswordHash
            };
            return dto;
        }

        // Other functions
        private async Task<List<User>> LoadAllUsersAsync()
        {
            try
            {
                var dtos = await _fileStorage.LoadAsync<List<UserDto>>(_filePath);

                return dtos
                    .Select(MapToDomain)
                    .ToList();
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning("User storage file not found. Creating new empty storage.");
                return new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load users from file: {FilePath}", _filePath);
                return new List<User>();
            }
        }
        private async Task SaveAllUsersAsync(IEnumerable<User> users)
        {
            var dtos = users.Select(MapToDto).ToList();
            await _fileStorage.SaveAsync(_filePath, dtos);
        }
    }
}