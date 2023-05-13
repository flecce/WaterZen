using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WaterZenSimulator.Services.Impl;
using WaterZenSimulator.Services.Interface;

namespace WaterZenSimulator.Telegram
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(host =>
                {
                    host.AddJsonFile("appsettings.json");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ConsoleHostedService>();
                    services.AddSingleton<IMQTTService, MQTTService>();
                });
    }

    internal sealed class ConsoleHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IMQTTService _mqttService;

        public ConsoleHostedService(
            ILogger<ConsoleHostedService> logger,
            IHostApplicationLifetime appLifetime,
            IMQTTService mqttService)
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _mqttService = mqttService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Random r = new Random((int)DateTime.Now.Ticks);
            _logger.LogDebug($"Starting with arguments: {string.Join(" ", Environment.GetCommandLineArgs())}");
            await _mqttService.StartAsync();
            _appLifetime.ApplicationStarted.Register(async () =>
            {
                var isOpenWater = false;
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        Console.ReadKey();
                        isOpenWater = !isOpenWater;
                    }

                    if (isOpenWater)
                    {
                        var data = new WaterData
                        {
                            FlowRate = r.Next(3, 10),
                            Temperature = r.Next(40, 50),
                            WaterOn = false
                        };
                        await _mqttService.SendWaterData(data);

                        Console.WriteLine($"Scenario acqua aperta: {JsonSerializer.Serialize(data)}");
                    }
                    else
                    {
                        var data = new WaterData
                        {
                            FlowRate = 0,
                            Temperature = 20,
                            WaterOn = false
                        };
                        await _mqttService.SendWaterData(data);

                        Console.WriteLine($"Scenario acqua chiusa: {JsonSerializer.Serialize(data)}");
                    }
                    await Task.Delay(500);
                }
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}