using ToDoApp.Interfaces;
using ToDoApp.Models;
using ToDoApp.Models.Enums;
using ToDoApp.Models.Tasks;
using ToDoApp.Services;

//Utils
ILogger logger = new FileLogger();
IPasswordHasher passwordHasher = new Pbkdf2PasswordHasher();
IFileStorageService fileStorageService = new FileStorageService();

//Services
IUserRepository userRepository = new JsonUserRepository(logger, fileStorageService);
IAuthService authService = new AuthService(logger, userRepository, passwordHasher);
ITaskManager taskManager = new TaskManager(logger, userRepository, authService);

logger.Info("Application started succesfully!");

try 
{
    await authService.SignUpAsync("andrii", "1234");
}
catch(InvalidOperationException ex)
{
    logger.Error(ex.Message);
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

foreach (var task in taskManager.SortByDate())
{
    await taskManager.RemoveTaskAsync(task.Id);
}
