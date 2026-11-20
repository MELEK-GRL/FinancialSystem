using FinancialTransactions.Application.Accounts;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Infrastructure.Persistence;

namespace FinancialTransactions.Infrastructure.Repositories;

public sealed class AccountRepository(AppDbContext dbContext) : IAccountRepository
{
    public void Add(Account account) => dbContext.Accounts.Add(account);
}
