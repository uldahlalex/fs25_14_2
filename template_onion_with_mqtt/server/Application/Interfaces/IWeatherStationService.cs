using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Core.Domain.Entities;
using Infrastructure.MQTT.SubscriptionEventHandlers;

namespace Application.Interfaces;

public interface IWeatherStationService
{
    List<Devicelog> GetDeviceFeed(JwtClaims client);
    Task AddToDbAndBroadcast(DeviceLogDto? dto);
}