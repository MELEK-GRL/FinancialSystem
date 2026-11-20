using FinancialTransactions.Infrastructure.Persistence;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Persistence;

public class UnitOfWorkTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await using var dbContext = postgres.CreateDbContext();
        await dbContext.Database.ExecuteSqlRawAsync(
            "CREATE TABLE IF NOT EXISTS unit_of_work_probe (value int NOT NULL); TRUNCATE unit_of_work_probe;");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ExecuteInTransaction_Commits_WhenOperationSucceeds()
    {
        await using var dbContext = postgres.CreateDbContext();
        var unitOfWork = new UnitOfWork(dbContext);

        await unitOfWork.ExecuteInTransactionAsync(
            token => dbContext.Database.ExecuteSqlRawAsync("INSERT INTO unit_of_work_probe VALUES (1), (2)", token),
            CancellationToken.None);

        Assert.Equal(2, await CountRowsAsync());
    }

    [Fact]
    public async Task ExecuteInTransaction_RollsBackEverything_WhenOperationFails()
    {
        await using var dbContext = postgres.CreateDbContext();
        var unitOfWork = new UnitOfWork(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                await dbContext.Database.ExecuteSqlRawAsync("INSERT INTO unit_of_work_probe VALUES (1)", token);
                throw new InvalidOperationException("second step failed");
            }, CancellationToken.None));

        Assert.Equal(0, await CountRowsAsync());
    }

    [Fact]
    public async Task ExecuteInTransaction_ReturnsOperationResult()
    {
        await using var dbContext = postgres.CreateDbContext();
        var unitOfWork = new UnitOfWork(dbContext);

        var result = await unitOfWork.ExecuteInTransactionAsync(_ => Task.FromResult(42), CancellationToken.None);

        Assert.Equal(42, result);
    }

    private async Task<int> CountRowsAsync()
    {
        await using var dbContext = postgres.CreateDbContext();
        return await dbContext.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM unit_of_work_probe").SingleAsync();
    }
}
