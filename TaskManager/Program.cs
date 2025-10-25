using TaskManager.Interfaces;
using TaskManager.Models;
using TaskManager.Models.Enums;
using TaskManager.Models.Tasks;
using TaskManager.Services;

IFileStorageService fileStorageService = new FileStorageService();
IUserRepository userRepository = new JsonUserRepository(fileStorageService);
IPasswordHasher passwordHasher = new Pbkdf2PasswordHasher();
IAuthService authService = new AuthService(userRepository, passwordHasher);
ITaskManager taskManager = new TaskManager.Services.TaskManager(userRepository, authService);

try 
{
    await authService.SignUpAsync("andrii", "1234");
}
catch(InvalidOperationException ex)
{
    Console.WriteLine(ex.Message);
}

bool loggedIn = await authService.SignInAsync("andrii", "1234");
if (!loggedIn)
{
    Console.WriteLine("Login failed");
    return;
}

ITask newTask = new SimpleTask("Learn C#", "Study async/await", DateTime.Now.AddDays(3), Priority.Low);
await taskManager.AddTaskAsync(newTask);

Console.WriteLine("Task added successfully!");

foreach (var task in taskManager.SortByDate())
{
    Console.WriteLine($"{task}");
}
