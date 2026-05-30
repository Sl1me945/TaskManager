using Microsoft.Extensions.DependencyInjection;

namespace ToDoApp.Presentation
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = GenericHost.CreateHostBuilder(args).Build();

            using var scope = host.Services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<AppRunner>();
            try
            {
                await runner.RunAsync();
            }
            finally
            {
                await host.StopAsync();
            }
        }
    }
}