using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace WaterZen.Telegram
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(host =>
                {
                    host.AddJsonFile("appsettings.json");
                })
                .ConfigureServices((hostContext, services) =>
                {
                });
    }
}