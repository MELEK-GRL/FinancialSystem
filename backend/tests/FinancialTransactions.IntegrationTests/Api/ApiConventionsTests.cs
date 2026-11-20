using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransactions.IntegrationTests.Api;

public class ApiConventionsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client = factory
        .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddControllers().AddApplicationPart(typeof(SampleItemsController).Assembly)))
        .CreateClient();

    [Fact]
    public async Task OpenApiDocument_IsAvailable()
    {
        var document = await client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");

        Assert.Equal("Financial Transactions API", document.GetProperty("info").GetProperty("title").GetString());
    }

    [Fact]
    public async Task ScalarUi_IsAvailable()
    {
        var response = await client.GetAsync("/scalar/v1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Controllers_AreServedUnderLowercaseVersionedRoute()
    {
        var response = await client.GetAsync("/api/v1/sampleitems/7");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201WithLocationHeader()
    {
        var response = await client.PostAsync("/api/v1/sampleitems", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/api/v1/sampleitems/7", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Enums_AreSerializedAsStrings()
    {
        var item = await client.GetFromJsonAsync<JsonElement>("/api/v1/sampleitems/7");

        Assert.Equal("Active", item.GetProperty("status").GetString());
    }
}

public enum SampleStatus
{
    Passive,
    Active,
}

public sealed record SampleItem(int Id, SampleStatus Status);

public class SampleItemsController : ApiControllerBase
{
    [HttpGet("{id:int}")]
    public SampleItem Get(int id) => new(id, SampleStatus.Active);

    [HttpPost]
    public IActionResult Create() => CreatedAtAction(nameof(Get), new { id = 7 }, new SampleItem(7, SampleStatus.Active));
}
