using FinancialTransactions.Domain.Transactions;

namespace FinancialTransactions.Application.Transactions;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(Transaction transaction);
}
