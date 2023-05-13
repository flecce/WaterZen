using MediatR;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using WaterZen.Telegram.Application.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using WaterZen.Telegram.Application.Mediator.Messages;

namespace WaterZen.Telegram.Application.Services.Impl
{
    internal class DeviceData
    {
        public DateTime Time { get; set; }
        public ATC53305e ATC53305e { get; set; }
    }

    internal class ATC53305e
    {
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
    }

    internal class MQTTService : IMQTTService
    {
        private readonly string? _server;
        private readonly string? _topic;
        private readonly IMediator _mediator;

        public MQTTService(IConfiguration configuration, IMediator mediator)
        {
            _server = configuration.GetValue<string>("MQTT:Server");
            _topic = configuration.GetValue<string>("MQTT:Topic");
            _mediator = mediator;
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
                    var data = JsonSerializer.Deserialize<DeviceData>(e.ApplicationMessage.Payload);
                    if (data != null)
                    {
                        _mediator.Send(new EnvironmentData
                        {
                            Temperature = data.ATC53305e.Temperature,
                            Humidity = data.ATC53305e.Humidity
                        });
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
    }
}