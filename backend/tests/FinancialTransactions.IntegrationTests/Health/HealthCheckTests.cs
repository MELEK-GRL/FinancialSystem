using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FinancialTransactions.IntegrationTests.Health;

public class HealthCheckTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client = factory
        .WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = "Host=localhost;Port=1;Database=none;Username=none;Password=none;Timeout=2",
            })))
        .CreateClient();

    [Fact]
    public async Task Live_ReturnsHealthy_WithoutCheckingDependencies()
    {
        var response = await client.GetAsync("/health/live");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", body.GetProperty("status").GetString());
        Assert.Equal(0, body.GetProperty("checks").GetArrayLength());
    }

    [Fact]
    public async Task Ready_Returns503_WhenDatabaseIsUnreachable()
    {
        var response = await client.GetAsync("/health/ready");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var database = body.GetProperty("checks")[0];

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("database", database.GetProperty("name").GetString());
        Assert.Equal("Unhealthy", database.GetProperty("status").GetString());
    }
}
