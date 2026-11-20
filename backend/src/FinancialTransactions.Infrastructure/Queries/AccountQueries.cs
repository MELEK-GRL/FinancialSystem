using FinancialTransactions.Application.Accounts;
using FinancialTransactions.Application.Accounts.Queries;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Queries;

public sealed class AccountQueries(AppDbContext dbContext) : IAccountQueries
{
    public async Task<IReadOnlyList<AccountDto>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
        await Project(dbContext.Accounts.AsNoTracking()
                .Where(account => account.CustomerId == customerId)
                .OrderBy(account => account.CreatedAt))
            .ToListAsync(cancellationToken);

    public Task<AccountDto?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken) =>
        Project(dbContext.Accounts.AsNoTracking().Where(account => account.Id == accountId))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<AccountOwnerRow?> GetOwnerByAccountNoAsync(string accountNo, CancellationToken cancellationToken) =>
        (from account in dbContext.Accounts.AsNoTracking()
         join customer in dbContext.Customers.AsNoTracking() on account.CustomerId equals customer.Id
         where account.AccountNo == accountNo
         select new AccountOwnerRow(
             account.AccountNo,
             customer.FirstName,
             customer.LastName,
             account.Status == AccountStatus.Active,
             customer.Status == CustomerStatus.Active))
        .SingleOrDefaultAsync(cancellationToken);

    private static IQueryable<AccountDto> Project(IQueryable<Account> accounts) =>
        accounts.Select(account => new AccountDto(
            account.Id,
            account.AccountNo,
            account.CustomerId,
            account.Currency,
            account.Balance,
            account.Status,
            account.CreatedAt));
}
