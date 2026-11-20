using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FinancialTransactions.IntegrationTests.Infrastructure;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgresFixture postgres = new();

    public AppDbContext CreateDbContext() => postgres.CreateDbContext();

    public Task InitializeAsync() => postgres.InitializeAsync();

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = postgres.ConnectionString,
            }));
}
