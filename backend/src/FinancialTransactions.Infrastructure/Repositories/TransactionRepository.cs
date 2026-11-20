using FinancialTransactions.Application.Transactions;
using FinancialTransactions.Domain.Transactions;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Repositories;

public sealed class TransactionRepository(AppDbContext dbContext) : ITransactionRepository
{
    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Transactions.SingleOrDefaultAsync(transaction => transaction.Id == id, cancellationToken);

    public Task<Transaction?> GetByIdempotencyKeyAsync(Guid customerId, string idempotencyKey, CancellationToken cancellationToken) =>
        dbContext.Transactions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                transaction => transaction.InitiatedByCustomerId == customerId && transaction.IdempotencyKey == idempotencyKey,
                cancellationToken);

    public void Add(Transaction transaction) => dbContext.Transactions.Add(transaction);
}
