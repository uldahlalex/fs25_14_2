using Application.Models.Dtos;
using Core.Domain.Entities;

namespace Application.Services;

public class ServerBroadcastsLiveDataToDashboard : ApplicationBaseDto
{
    public List<Devicelog> Logs { get; set; }
    public override string eventType { get; set; } = nameof(ServerBroadcastsLiveDataToDashboard);
}