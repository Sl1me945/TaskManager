using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Interfaces;
using TaskManager.Models;
using TaskManager.Models.Tasks;

namespace TaskManager.Services
{
    public class TaskManager : ITaskManager
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        private User CurrentUser => _authService.CurrentUser
            ?? throw new InvalidOperationException("User not logged in.");

        public TaskManager(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task AddTaskAsync(ITask task)
        {
            CurrentUser.Tasks.Add(task);
            await _userRepository.UpdateAsync(CurrentUser);
        }
        public async Task RemoveTaskAsync(Guid taskId)
        {
            var task = CurrentUser.Tasks.FirstOrDefault(t => t.Id == taskId) ?? throw new InvalidOperationException("Task not found");
            CurrentUser.Tasks.Remove(task);
            await _userRepository.UpdateAsync(CurrentUser);
        }
        public async Task MarkAsCompletedAsync(Guid taskId)
        {
            var task = CurrentUser.Tasks.FirstOrDefault(t => t.Id == taskId) ?? throw new InvalidOperationException("Task not found");
            task.MarkAsCompleted();
            await _userRepository.UpdateAsync(CurrentUser);
        }
        public IEnumerable<ITask> Search(string keyword)
            => CurrentUser.Tasks.Where(t => t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                        || t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        public IEnumerable<ITask> SortByDate(bool ascending = true)
            => ascending ? CurrentUser.Tasks.OrderBy(t => t.DueDate) : CurrentUser.Tasks.OrderByDescending(t => t.DueDate);
        public IEnumerable<ITask> FilterByCompletion(bool isCompleted)
            => CurrentUser.Tasks.Where(t => t.IsCompleted == isCompleted);
    }
}
