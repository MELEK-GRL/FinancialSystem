using FinancialTransactions.Domain.Accounts;

namespace FinancialTransactions.Application.Accounts;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(Account account);
}
