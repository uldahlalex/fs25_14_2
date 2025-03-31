using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.Types;

namespace Infrastructure.Mqtt;

public interface IMqttMessageHandler
{
    abstract string TopicFilter { get; }
    abstract QualityOfService QoS { get; }
    void Handle(object? sender, OnMessageReceivedEventArgs args);
}