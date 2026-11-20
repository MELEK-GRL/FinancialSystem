using FinancialTransactions.Application.Common.Paging;
using FinancialTransactions.Infrastructure.Persistence;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Persistence;

public class QueryableExtensionsTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task ToPagedResult_ReturnsRequestedPageAndTotalCount()
    {
        await using var dbContext = postgres.CreateDbContext();
        var numbers = dbContext.Database
            .SqlQueryRaw<int>("SELECT generate_series(1, 45) AS \"Value\"")
            .OrderBy(value => value);

        var result = await numbers.ToPagedResultAsync(new PageRequest(Page: 3, Size: 20), CancellationToken.None);

        Assert.Equal([41, 42, 43, 44, 45], result.Items);
        Assert.Equal(45, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(3, result.Page);
    }

    [Fact]
    public async Task ToPagedResult_ReturnsEmptyItems_WhenPageIsBeyondTheEnd()
    {
        await using var dbContext = postgres.CreateDbContext();
        var numbers = dbContext.Database
            .SqlQueryRaw<int>("SELECT generate_series(1, 5) AS \"Value\"")
            .OrderBy(value => value);

        var result = await numbers.ToPagedResultAsync(new PageRequest(Page: 2, Size: 20), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(5, result.TotalCount);
    }
}
