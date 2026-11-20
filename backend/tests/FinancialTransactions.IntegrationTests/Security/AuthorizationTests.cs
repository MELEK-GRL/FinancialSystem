using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Api.Security;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransactions.IntegrationTests.Security;

public class AuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;
    private readonly HttpClient client;

    public AuthorizationTests(WebApplicationFactory<Program> baseFactory)
    {
        factory = baseFactory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddControllers().AddApplicationPart(typeof(AuthorizationTestController).Assembly)));
        client = factory.CreateClient();
    }

    [Fact]
    public async Task EndpointWithoutAttribute_RequiresLogin_ByDefault()
    {
        var response = await client.GetAsync("/test-auth/default");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/test-auth/admin", true, HttpStatusCode.OK)]
    [InlineData("/test-auth/admin", false, HttpStatusCode.Forbidden)]
    [InlineData("/test-auth/customer", false, HttpStatusCode.OK)]
    [InlineData("/test-auth/customer", true, HttpStatusCode.Forbidden)]
    public async Task Policies_AllowOnlyTheirRole(string path, bool asAdmin, HttpStatusCode expected)
    {
        var token = asAdmin ? AdminToken() : CustomerToken(Guid.NewGuid());

        var response = await SendAsync(path, token);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Forbidden_ReturnsProblemDetails()
    {
        var response = await SendAsync("/test-auth/admin", CustomerToken(Guid.NewGuid()));
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(ErrorCodes.Forbidden, problem.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Customer_CanAccessOwnResource_ButGets404ForOthers()
    {
        var customerId = Guid.NewGuid();
        var token = CustomerToken(customerId);

        var own = await SendAsync($"/test-auth/customers/{customerId}", token);
        var other = await SendAsync($"/test-auth/customers/{Guid.NewGuid()}", token);

        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, other.StatusCode);
    }

    [Fact]
    public async Task UnknownRoute_Returns401ForAnonymous_And404ForSignedInUser()
    {
        var anonymous = await client.GetAsync("/route-does-not-exist");
        var signedIn = await SendAsync("/route-does-not-exist", CustomerToken(Guid.NewGuid()));
        var problem = await signedIn.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, signedIn.StatusCode);
        Assert.Equal(ErrorCodes.NotFound, problem.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Admin_CanAccessAnyCustomersResource()
    {
        var response = await SendAsync($"/test-auth/customers/{Guid.NewGuid()}", AdminToken());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private Task<HttpResponseMessage> SendAsync(string path, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client.SendAsync(request);
    }

    private string AdminToken() => CreateToken(User.CreateAdmin("admin@example.com", "hash"));

    private string CustomerToken(Guid customerId) => CreateToken(User.CreateCustomerUser("c@example.com", "hash", customerId));

    private string CreateToken(User user) => factory.Services.GetRequiredService<IJwtTokenService>().Create(user).Token;
}

[ApiController]
[Route("test-auth")]
public class AuthorizationTestController(ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("default")]
    public IActionResult Default() => Ok();

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("admin")]
    public IActionResult AdminOnly() => Ok();

    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpGet("customer")]
    public IActionResult CustomerOnly() => Ok();

    [HttpGet("customers/{customerId:guid}")]
    public IActionResult CustomerResource(Guid customerId) =>
        currentUser.CanAccessCustomer(customerId) ? Ok() : NotFound();
}
