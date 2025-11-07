using ToDoApp.Interfaces;
using ToDoApp.Models.Enums;
using ToDoApp.Models.Tasks;
using ToDoApp.Services;


//Utils
ILogger logger = new FileLogger();
IPasswordHasher passwordHasher = new Pbkdf2PasswordHasher();
IFileStorage fileStorage = new FileStorage();
ITokenService tokenService = new TokenService();

//Services
IUserRepository userRepository = new JsonUserRepository(logger, fileStorage);
IAuthService authService = new AuthService(logger, userRepository, passwordHasher, tokenService);
ITaskManager taskManager = new TaskManager(logger, userRepository, tokenService);

logger.Info("Application started succesfully!");

try 
{
    await authService.SignUpAsync("andrii", "1234");
}
catch(InvalidOperationException ex)
{
    logger.Error(ex.Message);
}


var token = await authService.SignInAsync("andrii", "1234");


if (token == null)
{
    Console.WriteLine("Login failed");
    return;
}

ITask newTask = new SimpleTask("Learn C#", "Study async/await", DateTime.Now.AddDays(3), Priority.Low);
await taskManager.AddTaskAsync(token, newTask);

Console.WriteLine("Task added successfully!");

foreach (var task in await taskManager.SortByDateAsync(token))
{
    Console.WriteLine($"{task}");
}

foreach (var task in await taskManager.SortByDateAsync(token))
{
    await taskManager.RemoveTaskAsync(token, task.Id);
}
