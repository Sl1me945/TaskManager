using Microsoft.Extensions.Logging;
using ToDoApp.Application.Interfaces;
using ToDoApp.Application.Services;
using ToDoApp.Infrastructure.Repositories;
using ToDoApp.Infrastructure.Services;

namespace ToDoApp.Presentation
{
    public static class Bootstrapper
    {
        public static (IAuthService AuthService, ITaskManager TaskManager, ITokenService TokenService, ILogger Logger) Build()
        {
            // Шляхи до файлів
            const string userRepositoryFilePath = "data/users/users.json";
            const string taskRepositoryFilePath = "data/tasks/tasks.json";

            // Логування
            ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger loggerBootstrapper = factory.CreateLogger("Bootstrapper");

            // Утиліти
            ILogger logger = factory.CreateLogger("Program");
            IPasswordHasher passwordHasher = new Pbkdf2PasswordHasher();
            IFileStorage fileStorage = new FileStorage();
            ITokenService tokenService = new JwtTokenService();

            // Репозиторії
            IUserRepository userRepository = new FileUserRepository(
                factory.CreateLogger<FileUserRepository>(),
                fileStorage,
                userRepositoryFilePath
            );

            ITaskRepository taskRepository = new FileTaskRepository(
                factory.CreateLogger<FileTaskRepository>(),
                fileStorage,
                taskRepositoryFilePath
            );

            // Сервіси
            IAuthService authService = new AuthService(
                factory.CreateLogger<AuthService>(),
                userRepository,
                passwordHasher,
                tokenService
            );

            ITaskManager taskManager = new TaskManager(
                factory.CreateLogger<TaskManager>(),
                taskRepository,
                userRepository
            );

            loggerBootstrapper.LogInformation("Bootstrapper initialized successfully.");

            return (authService, taskManager, tokenService, logger);
        }
    }
}
