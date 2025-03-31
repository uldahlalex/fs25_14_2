using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Core.Domain.Entities;
using Infrastructure.MQTT.SubscriptionEventHandlers;

namespace Application.Services;

public class WeatherStationService(IWeatherStationRepository weatherStationRepository,
    IConnectionManager connectionManager) : IWeatherStationService
{
    public List<Devicelog> GetDeviceFeed(JwtClaims client)
    {
        return weatherStationRepository.GetRecentLogs();
    }

    public Task AddToDbAndBroadcast(DeviceLogDto? dto)
    {
        var deviceLog = new Devicelog()
        {
            Timestamp = DateTime.UtcNow,
            Deviceid = dto.DeviceId,
            Unit = dto.Unit,
            Value = dto.Value
        };
        weatherStationRepository.AddDeviceLog(deviceLog);
        var recentLogs = weatherStationRepository.GetRecentLogs();
        connectionManager.BroadcastToTopic("dashboard", recentLogs);
        return Task.CompletedTask;
    }
}