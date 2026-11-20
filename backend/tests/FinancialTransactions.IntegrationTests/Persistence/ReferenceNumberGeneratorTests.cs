using FinancialTransactions.Infrastructure.Persistence;
using FinancialTransactions.IntegrationTests.Infrastructure;

namespace FinancialTransactions.IntegrationTests.Persistence;

public class ReferenceNumberGeneratorTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task Next_ReturnsTxPlusIstanbulDatePlusEightDigits()
    {
        var reference = await NextAsync();
        var istanbulToday = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"));

        Assert.Matches("^TX[0-9]{16}$", reference);
        Assert.Equal(istanbulToday.ToString("yyyyMMdd"), reference[2..10]);
    }

    [Fact]
    public async Task Next_IncreasesWithEachCall()
    {
        var first = await NextAsync();
        var second = await NextAsync();

        Assert.True(long.Parse(second[10..]) > long.Parse(first[10..]));
    }

    [Fact]
    public async Task ConcurrentCalls_NeverReturnTheSameNumber()
    {
        var references = await Task.WhenAll(Enumerable.Range(0, 200).Select(_ => NextAsync()));

        Assert.Equal(200, references.Distinct().Count());
    }

    private async Task<string> NextAsync()
    {
        await using var dbContext = postgres.CreateDbContext();
        return await new ReferenceNumberGenerator(dbContext).NextAsync(CancellationToken.None);
    }
}
