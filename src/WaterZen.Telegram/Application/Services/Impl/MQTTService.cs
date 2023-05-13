using MediatR;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using System.Text.Json;
using WaterZen.Telegram.Application.Mediator.Messages;
using WaterZen.Telegram.Application.Services.Interfaces;

namespace WaterZen.Telegram.Application.Services.Impl
{
    internal class MQTTService : IMQTTService
    {
        private readonly string? _server;
        private readonly string? _topic;
        private readonly ShowerService _showerService;

        public MQTTService(IConfiguration configuration, ShowerService showerService)
        {
            _server = configuration.GetValue<string>("MQTT:Server");
            _topic = configuration.GetValue<string>("MQTT:Topic");
            _showerService = showerService;
        }

        public async Task Listen(CancellationToken cancellationToken)
        {
            var mqttFactory = new MqttFactory();
            var mqttClient = mqttFactory.CreateMqttClient();
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_server).Build();

            // Setup message handling before connecting so that queued messages
            // are also handled properly. When there is no event handler attached all
            // received messages get lost.
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("Received application message.");

                if (e.ApplicationMessage != null)
                {
                    var data = JsonSerializer.Deserialize<DeviceData>(e.ApplicationMessage.PayloadSegment);
                    if (data != null)
                    {
                        _showerService.CheckClosingSession();
                        if (!_showerService.IsSessionActive)
                        {
                            _showerService.StartSession();
                        }

                        _showerService.AddFlowRate(data.FlowRate);
                        _showerService.AddTemperature(data.Temperature);
                    }
                }

                return Task.CompletedTask;
            };

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(_topic);
                    })
                .Build();

            await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
        }

        private class DeviceData
        {
            public bool WaterOn { get; set; }
            public decimal Temperature { get; set; }
            public decimal FlowRate { get; set; }
        }
    }
}