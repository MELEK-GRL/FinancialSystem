using FinancialTransactions.Domain.Accounts;

namespace FinancialTransactions.Application.Accounts;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Account>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    Task<IReadOnlyList<Account>> GetForUpdateAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    void Add(Account account);
}
