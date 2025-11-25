using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ToDoApp.Application.Interfaces;
using ToDoApp.Application.Services;
using ToDoApp.Infrastructure.Repositories;
using ToDoApp.Infrastructure.Services;

namespace ToDoApp.Presentation
{
    public static class GenericHost
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Logging
                    services.AddLogging(cfg => cfg.AddConsole());

                    // Infrastructure
                    services.AddSingleton<IFileStorage, FileStorage>();
                    services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
                    services.AddSingleton<ITokenService, JwtTokenService>();

                    // Repositories - provide file paths here (adjust as needed)
                    services.AddScoped<IUserRepository>(sp =>
                        new FileUserRepository(
                            sp.GetRequiredService<ILogger<FileUserRepository>>(),
                            sp.GetRequiredService<IFileStorage>(),
                            Path.Combine(AppContext.BaseDirectory, "data", "users", "users.json")));

                    services.AddScoped<ITaskRepository>(sp =>
                        new FileTaskRepository(
                            sp.GetRequiredService<ILogger<FileTaskRepository>>(),
                            sp.GetRequiredService<IFileStorage>(),
                            Path.Combine(AppContext.BaseDirectory, "data", "tasks", "tasks.json")));

                    // Application services
                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<ITaskManager, TaskManager>();

                    // Console runner / application entry
                    services.AddSingleton<AppRunner>();
                });
    }
}