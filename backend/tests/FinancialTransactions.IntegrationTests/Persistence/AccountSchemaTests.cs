using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinancialTransactions.IntegrationTests.Persistence;

public class AccountSchemaTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task NewAccount_GetsAccountNoAndKeepsDecimalBalanceExactly()
    {
        var account = Account.Open((await CreateCustomerAsync()).Id);
        account.Credit(1234.56m);

        await using (var dbContext = postgres.CreateDbContext())
        {
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
        }

        await using var readContext = postgres.CreateDbContext();
        var saved = await readContext.Accounts.SingleAsync(a => a.Id == account.Id);

        Assert.Matches("^TR[0-9]{10}$", saved.AccountNo);
        Assert.Equal(1234.56m, saved.Balance);
        Assert.Equal("TRY", saved.Currency);
    }

    [Fact]
    public async Task CustomerCanHaveMultipleAccounts()
    {
        var customer = await CreateCustomerAsync();

        await using var dbContext = postgres.CreateDbContext();
        dbContext.Accounts.AddRange(Account.Open(customer.Id), Account.Open(customer.Id));
        await dbContext.SaveChangesAsync();

        Assert.Equal(2, await dbContext.Accounts.CountAsync(a => a.CustomerId == customer.Id));
    }

    [Fact]
    public async Task NegativeBalance_IsRejectedByDatabase()
    {
        var account = Account.Open((await CreateCustomerAsync()).Id);
        await using var dbContext = postgres.CreateDbContext();
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var error = await Assert.ThrowsAsync<PostgresException>(() => dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE accounts SET balance = -1 WHERE id = {account.Id}"));

        Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
    }

    [Fact]
    public async Task AccountForUnknownCustomer_IsRejectedByDatabase()
    {
        await using var dbContext = postgres.CreateDbContext();
        dbContext.Accounts.Add(Account.Open(Guid.NewGuid()));

        var error = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, ((PostgresException)error.InnerException!).SqlState);
    }

    private async Task<Customer> CreateCustomerAsync()
    {
        var customer = Customer.Create("Ayşe", "Yılmaz", TestData.ValidNationalId(), TestData.UniqueEmail(), "5321234567");
        await using var dbContext = postgres.CreateDbContext();
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();
        return customer;
    }
}
