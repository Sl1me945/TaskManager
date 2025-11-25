using Microsoft.Extensions.DependencyInjection;

namespace ToDoApp.Presentation
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = GenericHost.CreateHostBuilder(args).Build();
            await host.StartAsync();

            var runner = host.Services.GetRequiredService<AppRunner>();
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