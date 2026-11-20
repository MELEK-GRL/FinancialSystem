using FinancialTransactions.Application.Accounts;
using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Application.Common.Options;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FinancialTransactions.Infrastructure.Repositories;

public sealed class AccountRepository(AppDbContext dbContext, IOptions<TransactionOptions> options) : IAccountRepository
{
    public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Accounts.SingleOrDefaultAsync(account => account.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Account>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        await dbContext.Accounts.Where(account => ids.Contains(account.Id)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Account>> GetForUpdateAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        var accountIds = ids.Distinct().ToArray();
        var lockTimeout = $"{options.Value.LockTimeoutMilliseconds}ms";

        try
        {
            await dbContext.Database.ExecuteSqlAsync($"SELECT set_config('lock_timeout', {lockTimeout}, true)", cancellationToken);

            return await dbContext.Accounts
                .FromSql($"SELECT * FROM accounts WHERE id = ANY({accountIds}) ORDER BY id FOR NO KEY UPDATE")
                .ToListAsync(cancellationToken);
        }
        catch (Exception exception) when (IsLockTimeout(exception))
        {
            throw new LockTimeoutException(exception);
        }
    }

    private static bool IsLockTimeout(Exception? exception) =>
        exception is not null
        && (exception is PostgresException { SqlState: PostgresErrorCodes.LockNotAvailable } || IsLockTimeout(exception.InnerException));

    public void Add(Account account) => dbContext.Accounts.Add(account);
}
