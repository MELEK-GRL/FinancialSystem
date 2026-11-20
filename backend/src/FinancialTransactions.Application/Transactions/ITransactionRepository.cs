using FinancialTransactions.Domain.Transactions;

namespace FinancialTransactions.Application.Transactions;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Transaction?> GetByIdempotencyKeyAsync(Guid customerId, string idempotencyKey, CancellationToken cancellationToken);

    void Add(Transaction transaction);
}
