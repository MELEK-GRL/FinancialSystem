using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Transactions;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinancialTransactions.IntegrationTests.Persistence;

public class TransactionSchemaTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task CompletedAt_IsSetByDatabase_WhenTransactionLeavesPending()
    {
        var (customer, account, _) = await SetupAsync();
        var transaction = Transaction.CreateDeposit(UniqueReference(), account.Id, 150.75m, null, customer.Id, null);
        await using var dbContext = postgres.CreateDbContext();
        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();

        Assert.Null(transaction.CompletedAt);

        transaction.MarkSucceeded();
        await dbContext.SaveChangesAsync();

        Assert.NotNull(transaction.CompletedAt);
        Assert.True(transaction.CompletedAt >= transaction.CreatedAt);

        await using var readContext = postgres.CreateDbContext();
        var saved = await readContext.Transactions.SingleAsync(t => t.Id == transaction.Id);
        Assert.Equal(150.75m, saved.Amount);
        Assert.Equal(TransactionStatus.Success, saved.Status);
    }

    [Fact]
    public async Task TransferToSameAccount_IsRejectedByDatabase()
    {
        var (customer, account, _) = await SetupAsync();

        var error = await SaveExpectingErrorAsync(
            Transaction.CreateTransfer(UniqueReference(), account.Id, account.Id, 10m, null, customer.Id, null));

        Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
    }

    [Fact]
    public async Task DepositWithSourceAccount_IsRejectedByDatabase()
    {
        var (customer, account, other) = await SetupAsync();
        await using var dbContext = postgres.CreateDbContext();

        var error = await Assert.ThrowsAsync<PostgresException>(() => dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO transactions (id, reference_no, type, status, amount, currency, source_account_id, target_account_id, initiated_by_customer_id)
            VALUES (gen_random_uuid(), {UniqueReference()}, 'Deposit', 'Pending', 10, 'TRY', {other.Id}, {account.Id}, {customer.Id})
            """));

        Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
    }

    [Fact]
    public async Task ZeroAmount_IsRejectedByDatabase()
    {
        var (customer, account, _) = await SetupAsync();
        await using var dbContext = postgres.CreateDbContext();

        var error = await Assert.ThrowsAsync<PostgresException>(() => dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO transactions (id, reference_no, type, status, amount, currency, target_account_id, initiated_by_customer_id)
            VALUES (gen_random_uuid(), {UniqueReference()}, 'Deposit', 'Pending', 0, 'TRY', {account.Id}, {customer.Id})
            """));

        Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
    }

    [Fact]
    public async Task IdempotencyKey_IsUniquePerCustomer_ButSharedKeysAcrossCustomersAreAllowed()
    {
        var (customer, account, _) = await SetupAsync();
        var (otherCustomer, otherAccount, _) = await SetupAsync();

        await SaveAsync(Transaction.CreateDeposit(UniqueReference(), account.Id, 10m, null, customer.Id, "same-key"));
        await SaveAsync(Transaction.CreateDeposit(UniqueReference(), otherAccount.Id, 10m, null, otherCustomer.Id, "same-key"));
        await SaveAsync(Transaction.CreateDeposit(UniqueReference(), account.Id, 10m, null, customer.Id, null));
        await SaveAsync(Transaction.CreateDeposit(UniqueReference(), account.Id, 10m, null, customer.Id, null));

        var error = await SaveExpectingErrorAsync(
            Transaction.CreateDeposit(UniqueReference(), account.Id, 10m, null, customer.Id, "same-key"));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, error.SqlState);
    }

    private async Task<(Customer Customer, Account Account, Account Other)> SetupAsync()
    {
        var customer = Customer.Create("Ayşe", "Yılmaz", TestData.ValidNationalId(), TestData.UniqueEmail(), "5321234567");
        var account = Account.Open(customer.Id);
        var other = Account.Open(customer.Id);
        await using var dbContext = postgres.CreateDbContext();
        dbContext.Customers.Add(customer);
        dbContext.Accounts.AddRange(account, other);
        await dbContext.SaveChangesAsync();
        return (customer, account, other);
    }

    private async Task SaveAsync(Transaction transaction)
    {
        await using var dbContext = postgres.CreateDbContext();
        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();
    }

    private async Task<PostgresException> SaveExpectingErrorAsync(Transaction transaction)
    {
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(transaction));
        return Assert.IsType<PostgresException>(error.InnerException);
    }

    private static string UniqueReference() => $"T{Guid.NewGuid():N}"[..20];
}
