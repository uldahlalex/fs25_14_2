using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
public class SubscriptionController(
    ISecurityService securityService, 
    IConnectionManager connectionManager,
    IWebsocketSubscriptionService websocketSubscriptionService) : ControllerBase
{
    public const string SubscriptionRoute = nameof(Subscribe);
    [HttpPost]
    [Route(SubscriptionRoute)]
    public async Task<ActionResult> Subscribe([FromHeader] string authorization, [FromBody]ChangeSubscriptionDto dto)
    {
        securityService.VerifyJwtOrThrow(authorization);
        await websocketSubscriptionService.SubscribeToTopic(dto.ClientId, dto.TopicIds);
        return Ok();
    }

    public const string UnsubscribeRoute = nameof(Unsubscribe);
    [HttpPost]
    [Route(UnsubscribeRoute)]
    public async Task<ActionResult> Unsubscribe([FromHeader] string authorization, [FromBody]ChangeSubscriptionDto dto)
    {
        securityService.VerifyJwtOrThrow(authorization);
        await websocketSubscriptionService.UnsubscribeFromTopic(dto.ClientId, dto.TopicIds);
        return Ok();
    }

    public const string ExampleBroadcastRoute = nameof(ExampleBroadcast);
    [HttpPost]
    [Route(ExampleBroadcastRoute)]

    public async Task<ActionResult> ExampleBroadcast([FromBody]ExampleBroadcastDto dto)
    {
        await connectionManager.BroadcastToTopic("ExampleTopic", dto);
        return Ok();
    }
}

public class ExampleBroadcastDto
{
    public string eventType { get; set; } = nameof(ExampleBroadcastDto);
    public string Message { get; set; }
}