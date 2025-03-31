using System.Text.Json;
using Application.Interfaces.Infrastructure.MQTT;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

namespace Infrastructure.Mqtt.PublishingHandlers;


public class MqttPublisher(HiveMQClient client) : IMqttPublisher
{ 
    public Task Publish(object dto, string topic)
    {
        client.PublishAsync(topic, JsonSerializer.Serialize(dto, new JsonSerializerOptions() 
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), QualityOfService.AtLeastOnceDelivery);
        return Task.CompletedTask;
    }
}