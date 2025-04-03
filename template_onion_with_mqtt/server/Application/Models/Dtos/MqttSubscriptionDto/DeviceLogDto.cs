using System.ComponentModel.DataAnnotations;

namespace Infrastructure.MQTT.SubscriptionEventHandlers;

public class DeviceLogDto
{
    [Required] [MinLength(1)] public string Unit { get; set; }
    [Required] [MinLength(1)] public int Value { get; set; }
    [Required] [MinLength(1)] public string DeviceId { get; set; }
}