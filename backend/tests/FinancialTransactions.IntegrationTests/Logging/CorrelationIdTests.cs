using FinancialTransactions.Api.Middleware;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FinancialTransactions.IntegrationTests.Logging;

public class CorrelationIdTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Response_ContainsIncomingCorrelationId_WhenValid()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CorrelationIdMiddleware.HeaderName, "order-123");

        var response = await client.GetAsync("/");

        Assert.Equal("order-123", ReadCorrelationId(response));
    }

    [Fact]
    public async Task Response_ContainsNewCorrelationId_WhenHeaderIsMissing()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.True(Guid.TryParse(ReadCorrelationId(response), out _));
    }

    [Fact]
    public async Task Response_ContainsNewCorrelationId_WhenIncomingValueIsInvalid()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, "bad id <script>");

        var response = await client.GetAsync("/");

        Assert.True(Guid.TryParse(ReadCorrelationId(response), out _));
    }

    private static string ReadCorrelationId(HttpResponseMessage response) =>
        response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single();
}
