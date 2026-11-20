using FinancialTransactions.Application.Transactions;
using FinancialTransactions.Application.Transactions.Queries;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Queries;

public sealed class TransactionQueries(AppDbContext dbContext) : ITransactionQueries
{
    public Task<TransactionWithOwners?> GetByReferenceNoAsync(string referenceNo, CancellationToken cancellationToken) =>
        (from transaction in dbContext.Transactions.AsNoTracking()
         where transaction.ReferenceNo == referenceNo
         from source in dbContext.Accounts.AsNoTracking().Where(account => account.Id == transaction.SourceAccountId).DefaultIfEmpty()
         from target in dbContext.Accounts.AsNoTracking().Where(account => account.Id == transaction.TargetAccountId).DefaultIfEmpty()
         select new TransactionWithOwners(
             new TransactionDto(
                 transaction.Id,
                 transaction.ReferenceNo,
                 transaction.Type,
                 transaction.Status,
                 transaction.Amount,
                 transaction.Currency,
                 transaction.SourceAccountId,
                 transaction.TargetAccountId,
                 transaction.Description,
                 transaction.FailureReason,
                 transaction.CreatedAt,
                 transaction.CompletedAt),
             transaction.InitiatedByCustomerId,
             source == null ? null : source.CustomerId,
             target == null ? null : target.CustomerId))
        .SingleOrDefaultAsync(cancellationToken);
}
