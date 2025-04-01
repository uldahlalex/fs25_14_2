using Api.Websocket;
using WebSocketBoilerplate;

namespace Startup.Tests;

/// <summary>
/// Service to add to DI with all the WS client details
/// </summary>
public class TestWsClient
{
    public WsRequestClient WsRequestClient { get; set; }
    public string WsClientId { get; set; }

    public TestWsClient()
    {
        var wsPort = Environment.GetEnvironmentVariable("PORT");
        if (string.IsNullOrEmpty(wsPort)) throw new Exception("Environment variable PORT is not set");
        WsClientId = Guid.NewGuid().ToString();
        var url = "ws://localhost:" + wsPort + "?id=" + WsClientId;
        WsRequestClient = new WsRequestClient(
            new[] { typeof(ServerSendsErrorMessage).Assembly },
            url
        );
        WsRequestClient.ConnectAsync().GetAwaiter().GetResult();
        Task.Delay(1000).GetAwaiter().GetResult();
    }

}