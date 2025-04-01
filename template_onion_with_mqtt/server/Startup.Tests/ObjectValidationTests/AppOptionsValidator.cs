using System.ComponentModel.DataAnnotations;
using Application.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.ObjectValidationTests;

public class AppOptionsValidator()  : WebApplicationFactory<Program>
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
        builder.ConfigureServices(services =>
        {
            services.DefaultTestConfig();
        });
    }

    [Test]
    public Task AppOptionsValidatorThrowsException()
    {
        var opts = new AppOptions();
        var context = new ValidationContext(opts, null, null);
        Assert.Throws<ValidationException>(() => Validator.ValidateObject(opts, context));
        return Task.CompletedTask;
    }

    [Test]
    public Task AppOptionsValidatorAcceptsValidAppOptions()
    {
        var opts = new AppOptions
        {
            DbConnectionString = "abc",
            JwtSecret = "abc",
            Seed = true
        };
        var context = new ValidationContext(opts, null, null);
        Validator.ValidateObject(opts, context); //Does not throw
        return Task.CompletedTask;
    }
}