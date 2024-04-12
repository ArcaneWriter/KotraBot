using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace KotraBot
{
    internal class Program
    {
        public static bool debug = false;

        private static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
#if DEBUG
            debug = true;
#endif

            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build();


            var serviceProvider = new ServiceCollection()
                .AddLogging(options =>
                {
                    options.ClearProviders();
                    options.AddConsole();
                })
                .AddSingleton<IConfiguration>(configuration)
                .AddScoped<IBot, Bot>()
                .BuildServiceProvider();

            try
            {
                IBot bot = serviceProvider.GetRequiredService<IBot>();

                await bot.StartAsync(serviceProvider);

                while (true)
                {
                    var keyInfo = Console.ReadKey();

                    if (keyInfo.Key == ConsoleKey.Q)
                    {
                        await bot.StopAsync();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(-1);
            }
        }
    }
}
