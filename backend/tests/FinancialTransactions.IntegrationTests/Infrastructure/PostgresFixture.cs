using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FinancialTransactions.IntegrationTests.Infrastructure;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public string ConnectionString => container.GetConnectionString();

    public AppDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options);

    public Task InitializeAsync() => container.StartAsync();

    public Task DisposeAsync() => container.DisposeAsync().AsTask();
}
