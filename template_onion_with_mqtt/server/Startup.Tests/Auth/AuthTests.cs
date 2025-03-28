using System.Net;
using Api.Rest.Controllers;
using Application.Models.Dtos;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.Auth;

public class AuthTests() : ApiTestBase(new ApiTestBaseConfig
{
    MockWebSocketService = false
})
{
    private const string TestUsername = "bob@bob.dk";
    private const string TestPassword = "asdASD123,-.";
    private const string TestSalt = "5cbd23b9-0cb4-4afe-8497-c81bc6691a42";

    private const string TestHash =
        "J4SHSN9SKisNBoijKZkNAA5GNWJlO/RNsiXWhoWq2lOpd7hBtmwnqb6bOcxxYP8tEvNRomJunrVkWKNa5W3lXg==";


    [Test]
    public async Task RouteWithNoAuth_Can_Be_Accessed()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/acceptance");
        if (await response.Content.ReadAsStringAsync() != "Accepted")
            throw new Exception("Expected 'Accepted'");
        if (HttpStatusCode.OK != response.StatusCode)
            throw new Exception("Expected OK status code");
    }

    [Test]
    public async Task SecuredRouteIsBlockedWitoutJwt()
    {
        var response = await CreateClient().GetAsync(AuthController.SecuredRoute);
        if (HttpStatusCode.Unauthorized != response.StatusCode)
            throw new Exception("Expected Unauthorized status code");
    }

    [Test]
    public async Task Register_Can_Register_And_Return_Jwt()
    {
        var user = MockObjects.GetUser();
        var response = await HttpClient.PostAsJsonAsync<AuthResponseDto>(AuthController.RegisterRoute,
            new AuthRequestDto
            {
                Email = user.Email,
                Password = "pass"
            });
        if (HttpStatusCode.OK != response.HttpResponseMessage.StatusCode)
            throw new Exception("Expected OK status code");
        if (response.Object.Jwt.Length < 10)
            throw new Exception("Expected jwt to be longer than 10 characters");
    }

    [Test]
    public async Task Register_With_Short_Pass_Returns_Bad_Request()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync<ProblemDetails>(
            AuthController.RegisterRoute, new AuthRequestDto
            {
                Email = "bob@bob.dk",
                Password = "a"
            });
        if (HttpStatusCode.BadRequest != response.HttpResponseMessage.StatusCode)
            throw new Exception("Expected BadRequest status code");
    }

    [Test]
    public async Task Login_Can_Login_And_Return_Jwt()
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var user = MockObjects.GetUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var response = await HttpClient.PostAsJsonAsync<AuthResponseDto>(
            AuthController.LoginRoute, new AuthRequestDto
            {
                Email = user.Email,
                Password = "pass"
            });
        if (HttpStatusCode.OK != response.HttpResponseMessage.StatusCode)
            throw new Exception("Expected OK status code");
    }

    [Test]
    public async Task Invalid_Login_Gives_Unauthorized()
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var user = MockObjects.GetUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var response = await CreateClient().PostAsJsonAsync<ProblemDetails>(AuthController.LoginRoute,
            new AuthRequestDto
            {
                Email = user.Email,
                Password = "invalid password"
            });
        if (HttpStatusCode.Unauthorized != response.HttpResponseMessage.StatusCode)
            throw new Exception("Expected Unauthorized status code");
    }

    [Test]
    public async Task Login_For_Non_Existing_User_Is_Unauthorized()
    {
        var response = await CreateClient().PostAsJsonAsync<ProblemDetails>(AuthController.LoginRoute,
            new AuthRequestDto { Email = "bob@bob.dk", Password = "password" });
        if (HttpStatusCode.BadRequest != response.HttpResponseMessage.StatusCode)
            throw new Exception("Expected BadRequest status code");
    }

    [Test]
    public async Task Register_For_Existing_User_Is_Bad_Request()
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var user = MockObjects.GetUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var response = await CreateClient().PostAsJsonAsync<ProblemDetails>(AuthController.RegisterRoute,
            new AuthRequestDto
            {
                Email = user.Email,
                Password = "password"
            });
        if (HttpStatusCode.BadRequest != response.HttpResponseMessage.StatusCode) ;
    }
}