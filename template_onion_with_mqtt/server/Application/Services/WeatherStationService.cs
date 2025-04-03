using Application.Interfaces;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Core.Domain.Entities;
using Infrastructure.MQTT.SubscriptionEventHandlers;

namespace Application.Services;

public class WeatherStationService(
    IWeatherStationRepository weatherStationRepository,
    IMqttPublisher mqttPublisher,
    IConnectionManager connectionManager) : IWeatherStationService
{    
    public Task AddToDbAndBroadcast(DeviceLogDto? dto)
     {
         var deviceLog = new Devicelog()
         {
             Timestamp = DateTime.UtcNow,
             Deviceid = dto.DeviceId,
             Unit = dto.Unit,
             Value = dto.Value,
             Id = Guid.NewGuid().ToString()
         };
         weatherStationRepository.AddDeviceLog(deviceLog);
         var recentLogs = weatherStationRepository.GetRecentLogs();
         var broadcast = new ServerBroadcastsLiveDataToDashboard()
         {
             Logs = recentLogs,
         };
         connectionManager.BroadcastToTopic(StringConstants.Dashboard, broadcast);
         return Task.CompletedTask;
     }
    public List<Devicelog> GetDeviceFeed(JwtClaims client)
    {
        return weatherStationRepository.GetRecentLogs();
    }



    public Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims)
    {
        mqttPublisher.Publish(dto, StringConstants.Device+$"/{dto.DeviceId}/"+StringConstants.ChangePreferences);
        return Task.CompletedTask;
    }
}