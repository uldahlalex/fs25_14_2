using System.Net.Http.Json;
using System.Text.Json;
using Api.Rest.Controllers;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos;
using Application.Services;
using Fleck;
using Infrastructure.MQTT.SubscriptionEventHandlers;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;
using WebSocketBoilerplate;

namespace Startup.Tests.EventTests;

[TestFixture]
public class EventTests
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
    
    [Test]
    public async Task WhenConnectingToApi_ServerAddsWsConnection_CanBeRetrievedById()
    {
        var connectionManager = _scopedServiceProvider.GetRequiredService<IConnectionManager>();
        _ = (IWebSocketConnection)connectionManager.GetSocketFromClientId(_scopedServiceProvider.GetRequiredService<TestWsClient>().WsClientId);
    }

    [Test]
    public async Task WhenSubscribingToTopicUsingRestRequest_ResponseIsOkAndConnectionManagerHasAddedToTopic()
    {   
        //Arrange
        var connectionManager = _scopedServiceProvider.GetService<IConnectionManager>();
        var initialMembers = await connectionManager.GetMembersFromTopicId("dashboard");
        if (initialMembers.Count != 0)
            throw new Exception("Initial members in topic should be 0, but it was: " +
                                JsonSerializer.Serialize(initialMembers));
        var registerDto = new AuthRequestDto()
        {
            Email = new Random().NextDouble() * 123 + "@gmail.com",
            Password = new Random().NextDouble() * 123 + "@gmail.com"
        };
        var signIn = await _httpClient.PostAsJsonAsync(
            AuthController.RegisterRoute, registerDto);
        var authResponseDto = await signIn.Content
                                  .ReadFromJsonAsync<AuthResponseDto>(new JsonSerializerOptions()
                                      { PropertyNameCaseInsensitive = true }) ??
                              throw new Exception("Failed to deserialize " + await signIn.Content.ReadAsStringAsync() +
                                                  " to " + nameof(AuthResponseDto));
        _httpClient.DefaultRequestHeaders.Add("Authorization", authResponseDto.Jwt);
        
        //Act
        var subscribeToTopicRequest = await _httpClient.PostAsJsonAsync(
            WeatherStationController.SubscribeToLiveChangesRoute, new SubscribeToTopicDto()
            {
                ClientId = _scopedServiceProvider.GetRequiredService<TestWsClient>().WsClientId,
                Topic = "dashboard"
            });
        
        //Assert
        if (!subscribeToTopicRequest.IsSuccessStatusCode)
            throw new Exception("Http response from subscription request indicates a failure to subscribe: "+ await subscribeToTopicRequest.Content.ReadAsStringAsync());
        var members = await connectionManager.GetMembersFromTopicId("dashboard");
        if (members.Count != 1)
            throw new Exception("Expected exactly one subscriber to topic dashboard, but this is the topic members: "+JsonSerializer.Serialize(members));
    }

    [Test]
    public async Task WhenServerReceivesTimeSeriesData_ServerSavesInDbAndBroadcastsToClient()
    {
        //Arrange
        var connectionManager = _scopedServiceProvider.GetService<IConnectionManager>();
        var wsClient = _scopedServiceProvider.GetService<TestWsClient>();
        await connectionManager.AddToTopic("dashboard", wsClient.WsClientId);
        var deviceId = "TestDevice"+new Random().NextDouble()*1234;
        var testDeviceLogObject = new DeviceLogDto()
        {
            DeviceId = deviceId,
            Unit = "Celcius",
            Value = 20
        };
        //Act
        await _scopedServiceProvider.GetRequiredService<IWeatherStationService>().AddToDbAndBroadcast(testDeviceLogObject);
        await Task.Delay(1000);
        
        //Assert
        var receivedDtos = wsClient.WsRequestClient.ReceivedMessagesAsJsonStrings
            .Select(str => JsonSerializer.Deserialize<BaseDto>(str));
        
        if (!receivedDtos.Any(baseDto => baseDto.eventType == nameof(ServerBroadcastsLiveDataToDashboard)))
            throw new Exception("Did not receive any websocket messages indicating a broadcast to the expected client");
        
        var dbCtx = _scopedServiceProvider.GetRequiredService<MyDbContext>();
        if (!dbCtx.Devicelogs.Any(log => log.Deviceid.Equals(deviceId)))
            throw new Exception("Expected a log form device of ID " + deviceId + " but only found: " +
                                JsonSerializer.Serialize(dbCtx.Devicelogs.ToList()));
    }
}