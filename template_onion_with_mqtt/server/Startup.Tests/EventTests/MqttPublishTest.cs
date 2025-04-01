using System.Net.Http.Json;
using System.Net.Security;
using System.Text.Json;
using Api.Rest.Controllers;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using HiveMQtt.MQTT5.Types;
using KellermanSoftware.CompareNetObjects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.EventTests;

[TestFixture]
public class MqttPublishTest
{
    private HttpClient _httpClient;
    private IServiceProvider _scopedServiceProvider;

    [SetUp]
    public void Setup()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services => { services.DefaultTestConfig(); });
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
    public async Task WhenAdminChangesDevicePreferencesFromWebDashboard_MqttClientPublishesToEdgeDevice()
    {
        //Arrange MQTT client to perform publish on REST trigger
        var testMqttClient = _scopedServiceProvider.GetService<TestMqttClient>();
        var topic = StringConstants.Device + "/" + testMqttClient.DeviceId + "/" + StringConstants.ChangePreferences;
        await testMqttClient.MqttClient.SubscribeAsync(topic, QualityOfService.ExactlyOnceDelivery);

        //Arrange WS client
        var testWsClient = _scopedServiceProvider.GetRequiredService<TestWsClient>();
        var connectionManager = _scopedServiceProvider.GetRequiredService<IConnectionManager>();
        await connectionManager.AddToTopic(StringConstants.Dashboard, testWsClient.WsClientId);

        //Rest DTO
        var changePrefernecesDto = new AdminChangesPreferencesDto
        {
            DeviceId = testMqttClient.DeviceId,
            Interval = "Minute",
            Unit = "Celcius"
        };

        //Act
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);
        _ = await _httpClient.PostAsJsonAsync(WeatherStationController.AdminChangesPreferencesRoute, changePrefernecesDto);
        await Task.Delay(1000); // Hardcoded delay to account for network overhead to the edge device
        
        var actualObjectReceivedByMqttDevice = JsonSerializer.Deserialize<AdminChangesPreferencesDto>(testMqttClient.ReceivedMessages.First(), JsonSerializerOptions.Web);
        var comparison = new CompareLogic().Compare(actualObjectReceivedByMqttDevice, changePrefernecesDto);
        if (!comparison.AreEqual)
            throw new Exception("Comparison failed: "+comparison.DifferencesString);

    }
}