using Microsoft.IdentityModel.JsonWebTokens;
using ToDoApp.Interfaces;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class TaskManager : ITaskManager
    {
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public TaskManager(ILogger logger, IUserRepository userRepository, ITokenService tokenService)
        {
            _logger = logger;
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task AddTaskAsync(string token, ITask task)
        {
            _logger.Info($"Add task: {task.Id}");

            var user = await getUserFromToken(token);

            user.Tasks.Add(task);

            await _userRepository.UpdateAsync(user);
        }
        public async Task RemoveTaskAsync(string token, Guid taskId)
        {
            _logger.Info($"Remove task: {taskId}");

            var user = await getUserFromToken(token);

            var task = user.Tasks.FirstOrDefault(t => t.Id == taskId) 
                ?? throw new InvalidOperationException("Task not found");
            user.Tasks.Remove(task);

            await _userRepository.UpdateAsync(user);
        }
        public async Task MarkAsCompletedAsync(string token, Guid taskId)
        {
            _logger.Info($"Mark as completed task: {taskId}");

            var user = await getUserFromToken(token);

            var task = user.Tasks.FirstOrDefault(t => t.Id == taskId) 
                ?? throw new InvalidOperationException("Task not found");
            task.MarkAsCompleted();

            await _userRepository.UpdateAsync(user);
        }
        public async Task<IEnumerable<ITask>> SearchAsync(string token, string keyword)
        {
            var user = await getUserFromToken(token);
            return user.Tasks.Where(t => 
                t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        public async Task<IEnumerable<ITask>> SortByDateAsync(string token, bool ascending = true)
        {
            var user = await getUserFromToken(token);
            return ascending 
                ? user.Tasks.OrderBy(t => t.DueDate) 
                : user.Tasks.OrderByDescending(t => t.DueDate);
        }
        public async Task<IEnumerable<ITask>> FilterByCompletionAsync(string token, bool isCompleted)
        {
            var user = await getUserFromToken(token);
            return user.Tasks.Where(t => t.IsCompleted == isCompleted);
        }

        private async Task<User> getUserFromToken(string token)
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
