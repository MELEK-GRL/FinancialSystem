using FinancialTransactions.Application.Accounts;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Repositories;

public sealed class AccountRepository(AppDbContext dbContext) : IAccountRepository
{
    public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Accounts.SingleOrDefaultAsync(account => account.Id == id, cancellationToken);

    public void Add(Account account) => dbContext.Accounts.Add(account);
}
