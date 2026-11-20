using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Transactions;
using FinancialTransactions.Infrastructure.Outbox;
using FinancialTransactions.Infrastructure.Persistence;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Persistence;

public class OutboxTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task Event_IsCommittedTogetherWithTheTransaction_AsUnpublishedJson()
    {
        var (customer, account) = await SetupAsync();
        var transaction = Transaction.CreateDeposit($"T{Guid.NewGuid():N}"[..20], account.Id, 250.50m, null, customer.Id, null);

        await using (var dbContext = postgres.CreateDbContext())
        {
            await new UnitOfWork(dbContext).ExecuteInTransactionAsync(_ =>
            {
                dbContext.Transactions.Add(transaction);
                new EfOutbox(dbContext).Enqueue("transaction.completed", transaction.Id, new { transaction.ReferenceNo, transaction.Amount });
                return Task.CompletedTask;
            }, CancellationToken.None);
        }

        await using var readContext = postgres.CreateDbContext();
        var message = await readContext.OutboxMessages.SingleAsync(m => m.AggregateId == transaction.Id);
        var amount = await readContext.Database
            .SqlQuery<string>($"SELECT payload ->> 'amount' AS \"Value\" FROM outbox_messages WHERE aggregate_id = {transaction.Id}")
            .SingleAsync();

        Assert.Equal("transaction.completed", message.EventType);
        Assert.Null(message.PublishedAt);
        Assert.NotEqual(default, message.CreatedAt);
        Assert.Contains("\"referenceNo\"", message.Payload);
        Assert.Equal("250.50", amount);
    }

    [Fact]
    public async Task Event_IsRolledBack_WhenABusinessRuleFailsAfterItWasQueued()
    {
        var (_, account) = await SetupAsync();
        var aggregateId = Guid.NewGuid();

        await using (var dbContext = postgres.CreateDbContext())
        {
            var tracked = await dbContext.Accounts.SingleAsync(a => a.Id == account.Id);

            await Assert.ThrowsAsync<InsufficientBalanceException>(() =>
                new UnitOfWork(dbContext).ExecuteInTransactionAsync(_ =>
                {
                    new EfOutbox(dbContext).Enqueue("transaction.completed", aggregateId, new { Amount = 10m });
                    tracked.Debit(10m);
                    return Task.CompletedTask;
                }, CancellationToken.None));
        }

        await using var readContext = postgres.CreateDbContext();
        Assert.False(await readContext.OutboxMessages.AnyAsync(m => m.AggregateId == aggregateId));
        Assert.Equal(0m, (await readContext.Accounts.SingleAsync(a => a.Id == account.Id)).Balance);
    }

    [Fact]
    public async Task UnpublishedEvents_HaveAPartialIndex()
    {
        await using var dbContext = postgres.CreateDbContext();

        var definition = await dbContext.Database
            .SqlQuery<string>($"SELECT indexdef AS \"Value\" FROM pg_indexes WHERE indexname = 'ix_outbox_messages_unpublished_created_at'")
            .SingleAsync();

        Assert.Contains("WHERE (published_at IS NULL)", definition);
    }

    private async Task<(Customer Customer, Account Account)> SetupAsync()
    {
        var customer = Customer.Create("Ayşe", "Yılmaz", TestData.ValidNationalId(), TestData.UniqueEmail(), "5321234567");
        var account = Account.Open(customer.Id);
        await using var dbContext = postgres.CreateDbContext();
        dbContext.Customers.Add(customer);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();
        return (customer, account);
    }
}
