using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;

namespace Infrastructure.Postgres.Postgresql.Data;

public class WeatherStationRepository(MyDbContext ctx) : IWeatherStationRepository
{
    public List<Devicelog> GetRecentLogs() => ctx.Devicelogs.Take(5).ToList();
    public Devicelog AddDeviceLog(Devicelog deviceLog)
    {
        ctx.Devicelogs.Add(deviceLog);
        ctx.SaveChanges();
        return deviceLog;
    }
}