using Microsoft.Extensions.Configuration;
using MQTTnet.Client;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterZenSimulator.Services.Interface;
using System.Text.Json;

namespace WaterZenSimulator.Services.Impl
{
    internal class MQTTService : IMQTTService
    {
        private readonly string? _server;
        private readonly string? _topic;
        private IMqttClient _mqttClient;

        public MQTTService(IConfiguration configuration)
        {
            _server = configuration.GetValue<string>("MQTT:Server");
            _topic = configuration.GetValue<string>("MQTT:Topic");
     
        }

        public async  Task SendWaterData(WaterData dataToSend)
        {
            if (_mqttClient == null || !_mqttClient.IsConnected)
            {
                await StartAsync();
            }
            await _mqttClient.PublishAsync(new MqttApplicationMessage
            {
                Topic = _topic,
                //ContentType = "application/json",
                PayloadSegment = ASCIIEncoding.UTF8.GetBytes(JsonSerializer.Serialize(dataToSend))
            });
        }

        public async Task StartAsync()
        {
            var mqttFactory = new MqttFactory();
             _mqttClient = mqttFactory.CreateMqttClient();
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_server).Build();

            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        }
    }
}
