using Application.Interfaces.Infrastructure.Websocket;
using Infrastructure.Postgres;
using Infrastructure.Postgres.Scaffolding;
using Infrastructure.Websocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PgCtx;
using Startup.Proxy;

namespace Startup.Tests.TestUtils;

public static class ApiTestBase
{
    public static IServiceCollection DefaultTestConfig(
        this IServiceCollection services,
        bool useTestContainer = true,
        bool mockProxyConfig = true,
       // bool makeScopedConnectionManager = true,
        Action? customSeeder = null
    )
    {
        if (useTestContainer)
        {
            var db = new PgCtxSetup<MyDbContext>();
            RemoveExistingService<DbContextOptions<MyDbContext>>(services);
            services.AddDbContext<MyDbContext>(opt =>
            {
                opt.UseNpgsql(db._postgres.GetConnectionString());
                opt.EnableSensitiveDataLogging();
                opt.LogTo(_ => { });
            });
        }

        if (mockProxyConfig)
        {
            RemoveExistingService<IProxyConfig>(services);
            var mockProxy = new Mock<IProxyConfig>();
            services.AddSingleton(mockProxy.Object);
        }

        // if (makeScopedConnectionManager)
        // {
        //     RemoveExistingService<IConnectionManager>(services);
        //     services.AddScoped<IConnectionManager, WebSocketConnectionManager>();
        // }

        if (customSeeder is not null)
        {
            RemoveExistingService<ISeeder>(services);
            customSeeder.Invoke();
        }

        services.AddTransient<TestWsClient>();
        return services;
    }   

    private static void RemoveExistingService<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
            services.Remove(descriptor);
    }
}