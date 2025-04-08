using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.OpenApiTests;

public class OpenApiTests : WebApplicationFactory<Program>
{
    private HttpClient _httpClient;
    private IServiceProvider _scopedServiceProvider;

    [SetUp]
    public void Setup()
    {
        _httpClient = CreateClient();
        _scopedServiceProvider = Services.CreateScope().ServiceProvider;
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => { services.DefaultTestConfig(); });
    }

    [Test]
    public async Task CanGetJsonResponseFromOpenApi()
    {
        var response = await CreateClient().GetAsync("/openapi/v1.json");
        var document = await OpenApiDocument.FromJsonAsync(await response.Content.ReadAsStringAsync());
        if (document.Paths.Count == 0)
            throw new Exception("Expected paths to be present in the open api document");
    }
}