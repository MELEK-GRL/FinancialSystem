using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Users;
using FinancialTransactions.Infrastructure.Persistence;
using FinancialTransactions.Infrastructure.Repositories;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Persistence;

public class RegistrationRollbackTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task WhenUserInsertFails_CustomerIsNotSavedEither()
    {
        var takenEmail = TestData.UniqueEmail();
        await using (var setup = postgres.CreateDbContext())
        {
            setup.Users.Add(User.CreateAdmin(takenEmail, "hash"));
            await setup.SaveChangesAsync();
        }

        var customer = Customer.Create("Ayşe", "Yılmaz", TestData.ValidNationalId(), TestData.UniqueEmail(), "5321234567");
        await using var dbContext = postgres.CreateDbContext();
        var unitOfWork = new UnitOfWork(dbContext);
        var customers = new CustomerRepository(dbContext);
        var users = new UserRepository(dbContext);

        await Assert.ThrowsAsync<UniqueConstraintViolationException>(() =>
            unitOfWork.ExecuteInTransactionAsync(_ =>
            {
                customers.Add(customer);
                users.Add(User.CreateCustomerUser(takenEmail, "hash", customer.Id));
                return Task.CompletedTask;
            }, CancellationToken.None));

        await using var readContext = postgres.CreateDbContext();
        Assert.False(await readContext.Customers.AnyAsync(c => c.Id == customer.Id));
    }
}
