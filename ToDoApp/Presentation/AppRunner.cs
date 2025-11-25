using Microsoft.Extensions.Logging;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities.Tasks;
using ToDoApp.Domain.Enums;

namespace ToDoApp.Presentation
{
    public class AppRunner
    {
        private readonly IAuthService _authService;
        private readonly ITaskManager _taskManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AppRunner> _logger;

        private string? _currentToken;
        private Guid _currentUserId;
        private string _currentUsername = string.Empty;

        public AppRunner(
            IAuthService authService,
            ITaskManager taskManager,
            ITokenService tokenService,
            ILogger<AppRunner> logger)
        {
            _authService = authService;
            _taskManager = taskManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            try
            {
                _logger.LogInformation("ToDoApp started successfully");

                Console.Clear();
                PrintWelcomeBanner();

                // Main application loop
                while (true)
                {
                    if (string.IsNullOrEmpty(_currentToken))
                    {
                        await ShowAuthMenuAsync();
                    }
                    else
                    {
                        await ShowMainMenuAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Fatal error occurred");
                Console.WriteLine($"\nFatal error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static void PrintWelcomeBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║                                        ║");
            Console.WriteLine("║          ToDo Application CLI          ║");
            Console.WriteLine("║                                        ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        private async Task ShowAuthMenuAsync()
        {
            Console.WriteLine("\n═══ Authentication Menu ═══");
            Console.WriteLine("1. Sign In");
            Console.WriteLine("2. Sign Up");
            Console.WriteLine("3. Exit");
            Console.Write("\nSelect option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await SignInAsync();
                    break;
                case "2":
                    await SignUpAsync();
                    break;
                case "3":
                    Console.WriteLine("\nGoodbye!");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("\nInvalid option. Please try again.");
                    await Task.Delay(1500);
                    break;
            }
        }

        private async Task ShowMainMenuAsync()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"╔════════════════════════════════════════╗");
            Console.WriteLine($"║ Logged in as: {_currentUsername,-24} ║");
            Console.WriteLine($"╚════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\n═══ Main Menu ═══");
            Console.WriteLine("1. Add Task");
            Console.WriteLine("2. View All Tasks");
            Console.WriteLine("3. Update Task");
            Console.WriteLine("4. Delete Task");
            Console.WriteLine("5. Mark Task as Completed");
            Console.WriteLine("6. Search Tasks");
            Console.WriteLine("7. Sort Tasks by Date");
            Console.WriteLine("8. Filter Tasks by Completion");
            Console.WriteLine("9. Sign Out");
            Console.Write("\nSelect option: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await AddTaskAsync();
                        break;
                    case "2":
                        await ViewAllTasksAsync();
                        break;
                    case "3":
                        await UpdateTaskAsync();
                        break;
                    case "4":
                        await DeleteTaskAsync();
                        break;
                    case "5":
                        await MarkTaskCompletedAsync();
                        break;
                    case "6":
                        await SearchTasksAsync();
                        break;
                    case "7":
                        await SortTasksByDateAsync();
                        break;
                    case "8":
                        await FilterTasksByCompletionAsync();
                        break;
                    case "9":
                        SignOut();
                        break;
                    default:
                        Console.WriteLine("\nInvalid option. Please try again.");
                        await Task.Delay(1500);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing menu option");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        private async Task SignUpAsync()
        {
            Console.Clear();
            Console.WriteLine("═══ Sign Up ═══\n");

            Console.Write("Username: ");
            var username = Console.ReadLine();

            Console.Write("Password: ");
            var password = ReadPassword();

            Console.Write("\nConfirm Password: ");
            var confirmPassword = ReadPassword();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\nUsername and password cannot be empty.");
                Console.ResetColor();
                await Task.Delay(2000);
                return;
            }

            if (password != confirmPassword)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\nPasswords do not match.");
                Console.ResetColor();
                await Task.Delay(2000);
                return;
            }

            try
            {
                await _authService.SignUpAsync(username, password);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n\nAccount created successfully! Please sign in.");
                Console.ResetColor();
                _logger.LogInformation("User {Username} signed up successfully", username);
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n\nSign up failed: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Sign up failed for user {Username}", username);
                await Task.Delay(2000);
            }
        }

        private async Task SignInAsync()
        {
            Console.Clear();
            Console.WriteLine("═══ Sign In ═══\n");

            Console.Write("Username: ");
            var username = Console.ReadLine();

            Console.Write("Password: ");
            var password = ReadPassword();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\nUsername and password cannot be empty.");
                Console.ResetColor();
                await Task.Delay(2000);
                return;
            }

            try
            {
                var token = await _authService.SignInAsync(username, password);

                if (string.IsNullOrEmpty(token))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n\nInvalid username or password.");
                    Console.ResetColor();
                    await Task.Delay(2000);
                    return;
                }

                _currentToken = token;

                // Validate token to get user ID
                var validationResult = await _tokenService.ValidateTokenAsync(token);
                if (validationResult.IsValid && validationResult.ClaimsIdentity != null)
                {
                    var userIdClaim = validationResult.ClaimsIdentity.FindFirst("sub");
                    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                    {
                        _currentUserId = userId;
                        _currentUsername = username;

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n\nSign in successful!");
                        Console.ResetColor();
                        _logger.LogInformation("User {Username} signed in successfully", username);
                        await Task.Delay(1500);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n\nSign in failed: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Sign in failed for user {Username}", username);
                await Task.Delay(2000);
            }
        }

        private void SignOut()
        {
            // Revoke server-side token (if any) and clear client state
            try
            {
                _authService.SignOutAsync(_currentToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign out");
            }
            _currentToken = null;
            _currentUserId = Guid.Empty;
            _currentUsername = string.Empty;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nSigned out successfully.");
            Console.ResetColor();
            _logger.LogInformation("User signed out");
            Thread.Sleep(1500);
            Console.Clear();
        }

        private async Task AddTaskAsync()
        {
            Console.Clear();
            Console.WriteLine("═══ Add New Task ═══\n");

            Console.WriteLine("Select task type:");
            Console.WriteLine("1. Simple Task");
            Console.WriteLine("2. Work Task");
            Console.WriteLine("3. Recurring Task");
            Console.Write("\nSelect option: ");

            var taskTypeChoice = Console.ReadLine();

            Console.Write("\nTask Title: ");
            var title = Console.ReadLine();

            Console.Write("Task Description: ");
            var description = Console.ReadLine();

            Console.Write("Due Date (yyyy-MM-dd): ");
            var dueDateInput = Console.ReadLine();

            Console.WriteLine("\nPriority:");
            Console.WriteLine("1. Low");
            Console.WriteLine("2. Medium");
            Console.WriteLine("3. High");
            Console.Write("Select priority: ");
            var priorityChoice = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(title))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nTitle cannot be empty.");
                Console.ResetColor();
                await Task.Delay(2000);
                return;
            }

            if (!DateTime.TryParse(dueDateInput, out var dueDate))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nInvalid date format.");
                Console.ResetColor();
                await Task.Delay(2000);
                return;
            }

            Priority priority = priorityChoice switch
            {
                "1" => Priority.Low,
                "2" => Priority.Medium,
                "3" => Priority.High,
                _ => Priority.Medium
            };
            try
            {

                BaseTask? task = null;
                switch (taskTypeChoice)
                {
                    case "1":
                        task = new SimpleTask(title, description ?? string.Empty, dueDate, priority);
                        break;

                    case "2":
                        Console.Write("Project Name: ");
                        var projectName = Console.ReadLine();
                        task = new WorkTask(title, description ?? string.Empty, dueDate, priority, projectName ?? string.Empty);
                        break;

                    case "3":
                        Console.Write("Repeat interval in days: ");
                        if (int.TryParse(Console.ReadLine(), out var days))
                        {
                            task = new RecurringTask(title, description ?? string.Empty, dueDate, priority, TimeSpan.FromDays(days));
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nInvalid interval.");
                            Console.ResetColor();
                            await Task.Delay(2000);
                            return;
                        }
                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nInvalid task type.");
                        Console.ResetColor();
                        await Task.Delay(2000);
                        return;
                }

                if (task != null)
                {
                    task.UserId = _currentUserId;
                    await _taskManager.AddTaskAsync(_currentUserId, task);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nTask added successfully!");
                    Console.ResetColor();
                    _logger.LogInformation("Task {TaskId} added for user {UserId}", task.Id, _currentUserId);
                    await Task.Delay(1500);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nFailed to add task: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Failed to add task for user {UserId}", _currentUserId);
                await Task.Delay(2000);
            }
        }

        private async Task ViewAllTasksAsync()
        {
            Console.Clear();
            Console.WriteLine("═══ All Tasks ═══\n");

            try
            {
                var tasks = await _taskManager.SortByDateAsync(_currentUserId, false);
                var taskList = tasks.ToList();

                if (taskList.Count == 0)
                {
                    Console.WriteLine("No tasks found.");
                }
                else
                {
                    DisplayTaskList(taskList);
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to load tasks: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Failed to load tasks for user {UserId}", _currentUserId);
                await Task.Delay(2000);
            }
        }

        private async Task UpdateTaskAsync()
        {
            Console.Clear();
            Console.WriteLine("═══ Update Task ═══\n");

            try
            {
                var tasks = await _taskManager.SortByDateAsync(_currentUserId, false);
                var taskList = tasks.ToList();

                if (taskList.Count == 0)
                {
                    Console.WriteLine("No tasks found.");
                    await Task.Delay(2000);
                    return;
                }

                DisplayTaskList(taskList);

                Console.Write("\nEnter task number to update: ");
                if (!int.TryParse(Console.ReadLine(), out var taskNumber) || taskNumber < 1 || taskNumber > taskList.Count)
                {
                    Console.WriteLine("Invalid task number.");
                    await Task.Delay(2000);
                    return;
                }

                var taskToUpdate = taskList[taskNumber - 1];

                Console.Write($"\nNew Title (current: {taskToUpdate.Title}) [press Enter to keep]: ");
                var newTitle = Console.ReadLine();

                Console.Write($"New Description (current: {taskToUpdate.Description}) [press Enter to keep]: ");
                var newDescription = Console.ReadLine();

                Console.Write($"New Due Date (current: {taskToUpdate.DueDate:yyyy-MM-dd}) [press Enter to keep]: ");
                var newDueDateInput = Console.ReadLine();

                Console.WriteLine("\nNew Priority (current: {0}):", taskToUpdate.Priority);
                Console.WriteLine("1. Low");
                Console.WriteLine("2. Medium");
                Console.WriteLine("3. High");
                Console.Write("Select priority [press Enter to keep]: ");
                var priorityChoice = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(newTitle))
                    taskToUpdate.Title = newTitle;

                if (!string.IsNullOrWhiteSpace(newDescription))
                    taskToUpdate.Description = newDescription;

                if (!string.IsNullOrWhiteSpace(newDueDateInput))
                {
                    if (DateTime.TryParse(newDueDateInput, out var parsedDate))
                        taskToUpdate.DueDate = parsedDate;
                }

                if (!string.IsNullOrWhiteSpace(priorityChoice))
                {
                    taskToUpdate.Priority = priorityChoice switch
                    {
                        "1" => Priority.Low,
                        "2" => Priority.Medium,
                        "3" => Priority.High,
                        _ => taskToUpdate.Priority
                    };
                }

                // Handle task-specific properties
                if (taskToUpdate is WorkTask workTask)
                {
                    Console.Write($"New Project Name (current: {workTask.ProjectName}) [press Enter to keep]: ");
                    var newProjectName = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(newProjectName))
                        workTask.ProjectName = newProjectName;
                }
                else if (taskToUpdate is RecurringTask recurringTask)
                {
                    Console.Write($"New Repeat Interval in days (current: {recurringTask.RepeatInterval.Days}) [press Enter to keep]: ");
                    var newIntervalInput = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(newIntervalInput) && int.TryParse(newIntervalInput, out var days))
                        recurringTask.RepeatInterval = TimeSpan.FromDays(days);
                }

                await _taskManager.UpdateTaskAsync(_currentUserId, taskToUpdate);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nTask updated successfully!");
                Console.ResetColor();
                _logger.LogInformation("Task {TaskId} updated for user {UserId}", taskToUpdate.Id, _currentUserId);
                await Task.Delay(1500);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nFailed to update task: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Failed to update task for user {UserId}", _currentUserId);
                await Task.Delay(2000);
            }
        }

        private async Task DeleteTaskAsync()
        {
            Console.Clear();
            Console.WriteLine("═══ Delete Task ═══\n");

            try
            {
                var tasks = await _taskManager.SortByDateAsync(_currentUserId, false);
                var taskList = tasks.ToList();

                if (taskList.Count == 0)
                {
                    Console.WriteLine("No tasks found.");
                    await Task.Delay(2000);
                    return;
                }

                DisplayTaskList(taskList);

                Console.Write("\nEnter task number to delete: ");
                if (!int.TryParse(Console.ReadLine(), out var taskNumber) || taskNumber < 1 || taskNumber > taskList.Count)
                {
                    Console.WriteLine("Invalid task number.");
                    await Task.Delay(2000);
                    return;
                }

                var taskToDelete = taskList[taskNumber - 1];

                Console.Write($"\nAre you sure you want to delete '{taskToDelete.Title}'? (y/n): ");
                var confirmation = Console.ReadLine()?.ToLower();

                if (confirmation == "y" || confirmation == "yes")
                {
                    await _taskManager.RemoveTaskAsync(_currentUserId, taskToDelete.Id);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nTask deleted successfully!");
                    Console.ResetColor();
                    _logger.LogInformation("Task {TaskId} deleted for user {UserId}", taskToDelete.Id, _currentUserId);
                }
                else
                {
                    Console.WriteLine("\nDeletion cancelled.");
                }

                await Task.Delay(1500);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nFailed to delete task: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Failed to delete task for user {UserId}", _currentUserId);
                await Task.Delay(2000);
            }
        }

        private async Task MarkTaskCompletedAsync()
        {
            Console.Clear();
            Console.WriteLine("═══ Mark Task as Completed ═══\n");

            try
            {
                var tasks = await _taskManager.FilterByCompletionAsync(_currentUserId, false);
                var taskList = tasks.ToList();

                if (taskList.Count == 0)
                {
                    Console.WriteLine("No incomplete tasks found.");
                    await Task.Delay(2000);
                    return;
                }

                DisplayTaskList(taskList);

                Console.Write("\nEnter task number to mark as completed: ");
                if (!int.TryParse(Console.ReadLine(), out var taskNumber) || taskNumber < 1 || taskNumber > taskList.Count)
                {
                    Console.WriteLine("Invalid task number.");
                    await Task.Delay(2000);
                    return;
                }

                var taskToComplete = taskList[taskNumber - 1];
                await _taskManager.MarkAsCompletedAsync(_currentUserId, taskToComplete.Id);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nTask marked as completed!");
                Console.ResetColor();
                _logger.LogInformation("Task {TaskId} marked as completed for user {UserId}", taskToComplete.Id, _currentUserId);
                await Task.Delay(1500);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nFailed to mark task as completed: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Failed to mark task as completed for user {UserId}", _currentUserId);
                await Task.Delay(2000);
            }
        }

        private async Task SearchTasksAsync()
        {
            Console.Clear();
            Console.WriteLine("═══ Search Tasks ═══\n");

            Console.Write("Enter search keyword: ");
            var keyword = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                Console.WriteLine("Search keyword cannot be empty.");
                await Task.Delay(2000);
                return;
            }

            try
            {
                var tasks = await _taskManager.SearchAsync(_currentUserId, keyword);
                var taskList = tasks.ToList();

                Console.WriteLine($"\nSearch results for '{keyword}':\n");

                if (taskList.Count == 0)
                {
                    Console.WriteLine("No matching tasks found.");
                }
                else
                {
                    DisplayTaskList(taskList);
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Search failed: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Search failed for user {UserId}", _currentUserId);
                await Task.Delay(2000);
            }
        }

        private async Task SortTasksByDateAsync()
        {
            Console.Clear();
            Console.WriteLine("═══ Sort Tasks by Date ═══\n");
            Console.WriteLine("1. Ascending (oldest first)");
            Console.WriteLine("2. Descending (newest first)");
            Console.Write("\nSelect option: ");

            var choice = Console.ReadLine();
            bool ascending = choice == "1";

            try
            {
                var tasks = await _taskManager.SortByDateAsync(_currentUserId, ascending);
                var taskList = tasks.ToList();

                Console.WriteLine($"\nTasks sorted by date ({(ascending ? "ascending" : "descending")}):\n");

                if (taskList.Count == 0)
                {
                    Console.WriteLine("No tasks found.");
                }
                else
                {
                    DisplayTaskList(taskList);
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Sort failed: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Sort failed for user {UserId}", _currentUserId);
                await Task.Delay(2000);
            }
        }

        private async Task FilterTasksByCompletionAsync()
        {
            Console.Clear();
            Console.WriteLine("═══ Filter Tasks ═══\n");
            Console.WriteLine("1. Show completed tasks");
            Console.WriteLine("2. Show incomplete tasks");
            Console.Write("\nSelect option: ");

            var choice = Console.ReadLine();
            bool showCompleted = choice == "1";

            try
            {
                var tasks = await _taskManager.FilterByCompletionAsync(_currentUserId, showCompleted);
                var taskList = tasks.ToList();

                Console.WriteLine($"\n{(showCompleted ? "Completed" : "Incomplete")} tasks:\n");

                if (taskList.Count == 0)
                {
                    Console.WriteLine($"No {(showCompleted ? "completed" : "incomplete")} tasks found.");
                }
                else
                {
                    DisplayTaskList(taskList);
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Filter failed: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Filter failed for user {UserId}", _currentUserId);
                await Task.Delay(2000);
            }
        }

        private static void DisplayTaskList(List<BaseTask> tasks)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                var statusColor = task.IsCompleted ? ConsoleColor.Green : ConsoleColor.Yellow;
                var status = task.IsCompleted ? "✓" : "○";

                // Task type indicator
                string taskTypeIndicator = task switch
                {
                    SimpleTask => "[Simple]",
                    WorkTask => "[Work]",
                    RecurringTask => "[Recurring]",
                    _ => "[Task]"
                };

                Console.ForegroundColor = statusColor;
                Console.Write($"{i + 1}. [{status}] ");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{taskTypeIndicator} ");
                Console.ResetColor();

                Console.Write($"{task.Title}");

                // Priority indicator
                var priorityColor = task.Priority switch
                {
                    Priority.High => ConsoleColor.Red,
                    Priority.Medium => ConsoleColor.Yellow,
                    Priority.Low => ConsoleColor.Green,
                    _ => ConsoleColor.White
                };
                Console.ForegroundColor = priorityColor;
                Console.Write($" [{task.Priority}]");
                Console.ResetColor();

                // Due date info
                var daysUntilDue = (task.DueDate.Date - DateTime.Now.Date).Days;
                if (daysUntilDue < 0 && !task.IsCompleted)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($" [OVERDUE by {Math.Abs(daysUntilDue)} days]");
                    Console.ResetColor();
                }
                else if (daysUntilDue <= 3 && !task.IsCompleted)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($" [Due in {daysUntilDue} days]");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write($" (Due: {task.DueDate:yyyy-MM-dd})");
                }

                Console.WriteLine();

                // Description
                if (!string.IsNullOrWhiteSpace(task.Description))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"   {task.Description}");
                    Console.ResetColor();
                }

                // Task-specific details
                if (task is WorkTask workTask && !string.IsNullOrWhiteSpace(workTask.ProjectName))
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"   Project: {workTask.ProjectName}");
                    Console.ResetColor();
                }
                else if (task is RecurringTask recurringTask)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"   Repeats every: {recurringTask.RepeatInterval.Days} days");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }
        }

        private static string ReadPassword()
        {
            var password = string.Empty;
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[0..^1];
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            return password;
        }
    }
}
