using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Users;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinancialTransactions.IntegrationTests.Persistence;

public class CustomerAndUserSchemaTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    private const string UniqueViolation = "23505";
    private const string CheckViolation = "23514";

    [Fact]
    public async Task NewCustomer_GetsCustomerNoAndTimestampsFromDatabase()
    {
        var customer = NewCustomer();

        await using (var dbContext = postgres.CreateDbContext())
        {
            dbContext.Customers.Add(customer);
            await dbContext.SaveChangesAsync();
        }

        await using var readContext = postgres.CreateDbContext();
        var saved = await readContext.Customers.SingleAsync(c => c.Id == customer.Id);

        Assert.Matches("^C[0-9]{9}$", saved.CustomerNo);
        Assert.NotEqual(default, saved.CreatedAt);
        Assert.Equal(CustomerStatus.Active, saved.Status);
    }

    [Fact]
    public async Task UpdatedAt_IsRefreshedByTrigger()
    {
        var customer = NewCustomer();
        await using var dbContext = postgres.CreateDbContext();
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();
        var firstUpdatedAt = customer.UpdatedAt;

        await Task.Delay(20);
        customer.UpdateContact(customer.Email, "5559876543");
        await dbContext.SaveChangesAsync();

        Assert.True(customer.UpdatedAt > firstUpdatedAt);
    }

    [Fact]
    public async Task DuplicateNationalId_IsRejectedByDatabase()
    {
        var first = NewCustomer();
        var duplicate = Customer.Create("Ali", "Kaya", first.NationalId, UniqueEmail(), "5320000000");

        await SaveAsync(first);
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(duplicate));

        Assert.Equal(UniqueViolation, ((PostgresException)error.InnerException!).SqlState);
    }

    [Fact]
    public async Task InvalidNationalIdFormat_IsRejectedByDatabase()
    {
        var customer = Customer.Create("Ali", "Kaya", "0123456789A", UniqueEmail(), "5320000000");

        var error = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(customer));

        Assert.Equal(CheckViolation, ((PostgresException)error.InnerException!).SqlState);
    }

    [Fact]
    public async Task CustomerRoleWithoutCustomer_IsRejectedByDatabase()
    {
        await using var dbContext = postgres.CreateDbContext();

        var error = await Assert.ThrowsAsync<PostgresException>(() => dbContext.Database.ExecuteSqlRawAsync(
            "INSERT INTO users (id, email, password_hash, role) VALUES (gen_random_uuid(), 'x@example.com', 'hash', 'Customer')"));

        Assert.Equal(CheckViolation, error.SqlState);
    }

    [Fact]
    public async Task CustomerCanHaveOnlyOneUser()
    {
        var customer = NewCustomer();
        await SaveAsync(customer);
        await SaveAsync(User.CreateCustomerUser(UniqueEmail(), "hash", customer.Id));

        var error = await Assert.ThrowsAsync<DbUpdateException>(() =>
            SaveAsync(User.CreateCustomerUser(UniqueEmail(), "hash", customer.Id)));

        Assert.Equal(UniqueViolation, ((PostgresException)error.InnerException!).SqlState);
    }

    private async Task SaveAsync(object entity)
    {
        await using var dbContext = postgres.CreateDbContext();
        dbContext.Add(entity);
        await dbContext.SaveChangesAsync();
    }

    private static Customer NewCustomer() =>
        Customer.Create("Ayşe", "Yılmaz", UniqueNationalId(), UniqueEmail(), "5321234567");

    private static string UniqueEmail() => $"{Guid.NewGuid():N}@example.com";

    private static string UniqueNationalId() => $"1{Random.Shared.NextInt64(1_000_000_000, 9_999_999_999)}";
}
