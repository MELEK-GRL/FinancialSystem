using FinancialTransactions.Application.Customers;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Repositories;

public sealed class CustomerRepository(AppDbContext dbContext) : ICustomerRepository
{
    public Task<bool> ExistsAsync(string nationalId, string email, CancellationToken cancellationToken) =>
        dbContext.Customers.AnyAsync(customer => customer.NationalId == nationalId || customer.Email == email, cancellationToken);

    public void Add(Customer customer) => dbContext.Customers.Add(customer);
}
