using System.Collections.Concurrent;
using FinancialTransactions.Api.ErrorHandling;
using FinancialTransactions.IntegrationTests.ErrorHandling;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinancialTransactions.IntegrationTests.Logging;

public class ErrorLogLevelTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly RecordingLoggerProvider logs = new();

    [Theory]
    [InlineData("/test-errors/business-rule")]
    [InlineData("/test-errors/not-found")]
    [InlineData("/test-errors/conflict")]
    public async Task HandledErrors_AreNotLoggedAsErrors(string path)
    {
        await CreateClient().GetAsync(path);

        Assert.DoesNotContain(logs.Entries, entry => entry.Level >= LogLevel.Error);
    }

    [Fact]
    public async Task UnexpectedError_IsLoggedOnceAsError()
    {
        await CreateClient().GetAsync("/test-errors/unexpected");

        var error = Assert.Single(logs.Entries, entry => entry.Level >= LogLevel.Error);
        Assert.Equal(typeof(GlobalExceptionHandler).FullName, error.Category);
    }

    private HttpClient CreateClient() =>
        factory
            .WithWebHostBuilder(builder => builder
                .ConfigureTestServices(services => services.AddControllers().AddApplicationPart(typeof(ErrorTestController).Assembly))
                .ConfigureLogging(logging => logging.AddProvider(logs)))
            .CreateClient();

    private sealed class RecordingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<(string Category, LogLevel Level)> Entries { get; } = new();

        public ILogger CreateLogger(string categoryName) => new RecordingLogger(categoryName, Entries);

        public void Dispose()
        {
        }
    }

    private sealed class RecordingLogger(string category, ConcurrentQueue<(string Category, LogLevel Level)> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            entries.Enqueue((category, logLevel));
    }
}
