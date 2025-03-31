using System.Text.Json;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Core.Domain.Entities;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.Types;
using Infrastructure.Mqtt;

namespace Infrastructure.MQTT.SubscriptionEventHandlers;

public class DeviceLogEventHandler(IWeatherStationService weatherStationService) : IMqttMessageHandler
{
    public string TopicFilter { get; } = "device/+/log";
    public QualityOfService QoS { get; } = QualityOfService.AtLeastOnceDelivery;
    public void Handle(object? sender, OnMessageReceivedEventArgs args)
    {
        var dto = JsonSerializer.Deserialize<DeviceLogDto>(args.PublishMessage.PayloadAsString,
            new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
        weatherStationService.AddToDbAndBroadcast(dto);
    }
}