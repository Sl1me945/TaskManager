using TaskManager.Interfaces;
using TaskManager.Models;
using TaskManager.Models.Enums;
using TaskManager.Models.Tasks;
using TaskManager.Services;

IUserRepository userRepository = new InMemoryUserRepository();
IPasswordHasher passwordHasher = new Pbkdf2PasswordHasher();
IAuthService authService = new AuthService(userRepository, passwordHasher);
ITaskManager taskManager = new TaskManager.Services.TaskManager(userRepository, authService);

// Реєстрація
await authService.SignUpAsync("andrii", "1234");

// Вхід
bool loggedIn = await authService.SignInAsync("andrii", "1234");
if (!loggedIn)
{
    Console.WriteLine("Login failed");
    return;
}

// Додавання задачі
ITask newTask = new SimpleTask("Learn C#", "Study async/await", DateTime.Now.AddDays(3), Priority.Low);
await taskManager.AddTaskAsync(newTask);

Console.WriteLine("Task added successfully!");

// Перевірка
foreach (var task in taskManager.SortByDate())
{
    Console.WriteLine($"{task}");
}
