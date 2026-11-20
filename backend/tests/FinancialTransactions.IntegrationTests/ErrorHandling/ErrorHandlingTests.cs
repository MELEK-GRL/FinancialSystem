using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Api.Middleware;
using FinancialTransactions.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransactions.IntegrationTests.ErrorHandling;

public class ErrorHandlingTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client = factory
        .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddControllers().AddApplicationPart(typeof(ErrorTestController).Assembly)))
        .CreateClient();

    [Fact]
    public async Task BusinessRuleException_Returns422WithErrorCodeAndTurkishTitle()
    {
        var problem = await GetProblemAsync("/test-errors/business-rule");

        Assert.Equal(422, problem.GetProperty("status").GetInt32());
        Assert.Equal(ErrorCodes.InsufficientBalance, problem.GetProperty("errorCode").GetString());
        Assert.Equal("Yetersiz bakiye", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Title_IsEnglish_WhenAcceptLanguageIsEnglish()
    {
        var problem = await GetProblemAsync("/test-errors/business-rule", language: "en");

        Assert.Equal("Insufficient balance", problem.GetProperty("title").GetString());
    }

    [Theory]
    [InlineData("/test-errors/not-found", 404, ErrorCodes.AccountNotFound)]
    [InlineData("/test-errors/conflict", 409, ErrorCodes.DuplicateCustomer)]
    [InlineData("/route-does-not-exist", 404, ErrorCodes.NotFound)]
    public async Task Errors_AreMappedToStatusAndErrorCode(string path, int status, string errorCode)
    {
        var problem = await GetProblemAsync(path);

        Assert.Equal(status, problem.GetProperty("status").GetInt32());
        Assert.Equal(errorCode, problem.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task UnexpectedException_Returns500WithoutInternalDetails()
    {
        var response = await client.GetAsync("/test-errors/unexpected");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains(ErrorCodes.InternalError, body);
        Assert.DoesNotContain("connection string leaked", body);
    }

    [Fact]
    public async Task TraceId_MatchesCorrelationId()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/test-errors/business-rule");
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, "trace-123");

        var response = await client.SendAsync(request);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("trace-123", problem.GetProperty("traceId").GetString());
    }

    private async Task<JsonElement> GetProblemAsync(string path, string language = "tr")
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.AcceptLanguage.ParseAdd(language);

        var response = await client.SendAsync(request);

        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}

[ApiController]
[Route("test-errors")]
public class ErrorTestController : ControllerBase
{
    [HttpGet("business-rule")]
    public IActionResult BusinessRule() => throw new TestBusinessRuleException();

    [HttpGet("not-found")]
    public IActionResult NotFoundError() => throw new TestNotFoundException();

    [HttpGet("conflict")]
    public IActionResult ConflictError() => throw new TestConflictException();

    [HttpGet("unexpected")]
    public IActionResult Unexpected() => throw new InvalidOperationException("connection string leaked");
}

public sealed class TestBusinessRuleException() : BusinessRuleException(ErrorCodes.InsufficientBalance);

public sealed class TestNotFoundException() : NotFoundException(ErrorCodes.AccountNotFound);

public sealed class TestConflictException() : ConflictException(ErrorCodes.DuplicateCustomer);
