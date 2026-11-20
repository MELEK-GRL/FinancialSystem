using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Transactions;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinancialTransactions.IntegrationTests.Persistence;

public class TransactionImmutabilityTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task Transactions_CannotBeDeleted()
    {
        var transaction = await CreatePendingAsync();

        var error = await ExecuteExpectingErrorAsync($"DELETE FROM transactions WHERE id = '{transaction.Id}'");

        Assert.Contains("append-only", error.MessageText);
        Assert.True(await ExistsAsync(transaction.Id));
    }

    [Fact]
    public async Task Transactions_CannotBeTruncated()
    {
        await CreatePendingAsync();

        var error = await ExecuteExpectingErrorAsync("TRUNCATE transactions CASCADE");

        Assert.Contains("append-only", error.MessageText);
    }

    [Fact]
    public async Task PendingTransaction_CanBeCompleted_ThroughDomain()
    {
        var transaction = await CreatePendingAsync();
        await using var dbContext = postgres.CreateDbContext();
        var tracked = await dbContext.Transactions.SingleAsync(t => t.Id == transaction.Id);

        tracked.MarkFailed("INSUFFICIENT_BALANCE");
        await dbContext.SaveChangesAsync();

        Assert.Equal(TransactionStatus.Failed, tracked.Status);
        Assert.NotNull(tracked.CompletedAt);
    }

    [Theory]
    [InlineData("amount = 999")]
    [InlineData("description = 'changed'")]
    [InlineData("created_at = now() - interval '1 day'")]
    [InlineData("reference_no = 'CHANGED'")]
    public async Task PendingTransaction_CoreFieldsCannotChange(string change)
    {
        var transaction = await CreatePendingAsync();

        var error = await ExecuteExpectingErrorAsync($"UPDATE transactions SET {change} WHERE id = '{transaction.Id}'");

        Assert.Contains("only status and failure_reason", error.MessageText);
    }

    [Theory]
    [InlineData("status = 'Failed'")]
    [InlineData("failure_reason = 'changed later'")]
    [InlineData("amount = 1")]
    public async Task CompletedTransaction_CannotChangeAtAll(string change)
    {
        var transaction = await CreatePendingAsync();
        await using (var dbContext = postgres.CreateDbContext())
        {
            (await dbContext.Transactions.SingleAsync(t => t.Id == transaction.Id)).MarkSucceeded();
            await dbContext.SaveChangesAsync();
        }

        var error = await ExecuteExpectingErrorAsync($"UPDATE transactions SET {change} WHERE id = '{transaction.Id}'");

        Assert.Contains("cannot be changed", error.MessageText);
    }

    private async Task<Transaction> CreatePendingAsync()
    {
        var customer = Customer.Create("Ayşe", "Yılmaz", TestData.ValidNationalId(), TestData.UniqueEmail(), "5321234567");
        var account = Account.Open(customer.Id);
        var transaction = Transaction.CreateDeposit($"T{Guid.NewGuid():N}"[..20], account.Id, 100m, "Maaş", customer.Id, null);

        await using var dbContext = postgres.CreateDbContext();
        dbContext.Customers.Add(customer);
        dbContext.Accounts.Add(account);
        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();
        return transaction;
    }

    private async Task<PostgresException> ExecuteExpectingErrorAsync(string sql)
    {
        await using var dbContext = postgres.CreateDbContext();
        return await Assert.ThrowsAsync<PostgresException>(() => dbContext.Database.ExecuteSqlRawAsync(sql));
    }

    private async Task<bool> ExistsAsync(Guid id)
    {
        await using var dbContext = postgres.CreateDbContext();
        return await dbContext.Transactions.AnyAsync(t => t.Id == id);
    }
}
