using Application.Interfaces.Infrastructure.Websocket;
using Fleck;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.EventTests;

[TestFixture]
public class WsOnlyTests
{
    private HttpClient _httpClient;
    private IServiceProvider _scopedServiceProvider;

    [SetUp]
    public void Setup()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.DefaultTestConfig();
                });
            });

        _httpClient = factory.CreateClient();
        _scopedServiceProvider = factory.Services.CreateScope().ServiceProvider;
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }
    
    /// <summary>
    /// 
    /// </summary>
    [Test]
    public async Task WhenConnectingToApi_ServerAddsWsConnection_CanBeRetrievedById()
    {
        var connectionManager = _scopedServiceProvider.GetRequiredService<IConnectionManager>();
        var testWsClient = _scopedServiceProvider.GetRequiredService<TestWsClient>();
        _ = (IWebSocketConnection)connectionManager.GetSocketFromClientId(testWsClient.WsClientId);
    }


}