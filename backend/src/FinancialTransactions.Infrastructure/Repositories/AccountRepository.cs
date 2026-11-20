using FinancialTransactions.Application.Accounts;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Repositories;

public sealed class AccountRepository(AppDbContext dbContext) : IAccountRepository
{
    public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Accounts.SingleOrDefaultAsync(account => account.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Account>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        await dbContext.Accounts.Where(account => ids.Contains(account.Id)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Account>> GetForUpdateAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        var accountIds = ids.Distinct().ToArray();

        return await dbContext.Accounts
            .FromSql($"SELECT * FROM accounts WHERE id = ANY({accountIds}) ORDER BY id FOR UPDATE")
            .ToListAsync(cancellationToken);
    }

    public void Add(Account account) => dbContext.Accounts.Add(account);
}
