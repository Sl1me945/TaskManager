using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities;
using ToDoApp.Domain.Entities.Tasks;

namespace ToDoApp.Application.Services
{
    public class TaskManager(ILogger<TaskManager> logger, IUserRepository userRepository, ITokenService tokenService) : ITaskManager
    {
        private readonly ILogger<TaskManager> _logger = logger;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ITokenService _tokenService = tokenService;

        public async Task AddTaskAsync(string token, BaseTask task)
        {
            _logger.LogInformation($"Add task: {task.Id}");

            var user = await GetUserFromToken(token);

            user.Tasks.Add(task);

            await _userRepository.UpdateAsync(user);
        }
        public async Task RemoveTaskAsync(string token, Guid taskId)
        {
            _logger.LogInformation($"Remove task: {taskId}");

            var user = await GetUserFromToken(token);

            var task = user.Tasks.FirstOrDefault(t => t.Id == taskId) 
                ?? throw new InvalidOperationException("Task not found");
            user.Tasks.Remove(task);

            await _userRepository.UpdateAsync(user);
        }
        public async Task MarkAsCompletedAsync(string token, Guid taskId)
        {
            _logger.LogInformation($"Mark as completed task: {taskId}");

            var user = await GetUserFromToken(token);

            var task = user.Tasks.FirstOrDefault(t => t.Id == taskId) 
                ?? throw new InvalidOperationException("Task not found");
            task.MarkAsCompleted();

            await _userRepository.UpdateAsync(user);
        }
        public async Task<IEnumerable<BaseTask>> SearchAsync(string token, string keyword)
        {
            var user = await GetUserFromToken(token);
            return user.Tasks.Where(t => 
                t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        public async Task<IEnumerable<BaseTask>> SortByDateAsync(string token, bool ascending = true)
        {
            var user = await GetUserFromToken(token);
            return ascending 
                ? user.Tasks.OrderBy(t => t.DueDate) 
                : user.Tasks.OrderByDescending(t => t.DueDate);
        }
        public async Task<IEnumerable<BaseTask>> FilterByCompletionAsync(string token, bool isCompleted)
        {
            var user = await GetUserFromToken(token);
            return user.Tasks.Where(t => t.IsCompleted == isCompleted);
        }

        private async Task<User> GetUserFromToken(string token)
        {
            var validationResult = await _tokenService.ValidateTokenAsync(token);
            if (validationResult == null || !validationResult.IsValid)
                throw new UnauthorizedAccessException("Invalid or expired token.");

            var username = validationResult.ClaimsIdentity?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(username))
                throw new UnauthorizedAccessException("Token does not contain username.");

            var user = await _userRepository.GetByUsernameAsync(username)
                ?? throw new InvalidOperationException("User not found.");

            return user;
        }
    }
}
