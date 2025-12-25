using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ToDoApp.Application.Interfaces;
using ToDoApp.Application.Services;
using ToDoApp.Domain.Interfaces;
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

                    // Bind options
                    services.Configure<JwtOptions>(hostContext.Configuration.GetSection("Jwt"));

                    // Infrastructure
                    services.AddSingleton<IFileStorage, FileStorage>();
                    services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
                    services.AddSingleton<ITokenService, JwtTokenService>();

                    // Data file paths from configuration
                    var usersFileRelative = hostContext.Configuration.GetValue<string>("Data:UsersFile") ?? "data/users/users.json";
                    var tasksFileRelative = hostContext.Configuration.GetValue<string>("Data:TasksFile") ?? "data/tasks/tasks.json";
                    var usersFile = Path.Combine(AppContext.BaseDirectory, usersFileRelative);
                    var tasksFile = Path.Combine(AppContext.BaseDirectory, tasksFileRelative);

                    // Repositories - provide file paths here (adjust as needed)
                    services.AddScoped<IUserRepository>(sp =>
                        new FileUserRepository(
                            sp.GetRequiredService<ILogger<FileUserRepository>>(),
                            sp.GetRequiredService<IFileStorage>(),
                            usersFile));

                    services.AddScoped<ITaskRepository>(sp =>
                        new FileTaskRepository(
                            sp.GetRequiredService<ILogger<FileTaskRepository>>(),
                            sp.GetRequiredService<IFileStorage>(),
                            tasksFile));

                    // Application services
                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<ITaskManager, TaskManager>();

                    // Console runner / application entry
                    services.AddScoped<AppRunner>();
                });
    }
}