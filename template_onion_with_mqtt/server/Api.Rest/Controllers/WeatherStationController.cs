using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
public class WeatherStationController(
    IWeatherStationService weatherStationService, 
    IConnectionManager connectionManager,
    ISecurityService securityService) : ControllerBase
{
    
    public const string GetLogsRoute = nameof(GetLogs);

    [HttpGet]
    [Route(GetLogsRoute)]
    public async Task<ActionResult<IEnumerable<Devicelog>>> GetLogs([FromHeader]string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var feed = weatherStationService.GetDeviceFeed(claims);
        return Ok(feed);
    }

    public const string SubscribeToLiveChangesRoute = nameof(SubscribeToLiveChanges);
    [HttpPost]
    [Route(SubscribeToLiveChangesRoute)]
    public async Task<ActionResult> SubscribeToLiveChanges([FromHeader] string authorization, [FromBody]SubscribeToTopicDto dto)
    {
        securityService.VerifyJwtOrThrow(authorization);
        await connectionManager.AddToTopic(dto.Topic, dto.ClientId);
        return Ok();
    }
}