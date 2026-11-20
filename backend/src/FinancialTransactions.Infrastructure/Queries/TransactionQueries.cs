using FinancialTransactions.Application.Common.Paging;
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

    public Task<PagedResult<AccountTransactionDto>> GetAccountHistoryAsync(
        Guid accountId,
        TransactionHistoryFilter filter,
        TransactionHistorySort sort,
        PageRequest page,
        CancellationToken cancellationToken)
    {
        var transactions = dbContext.Transactions.AsNoTracking()
            .Where(transaction => transaction.SourceAccountId == accountId || transaction.TargetAccountId == accountId);

        if (filter.CreatedFrom is { } createdFrom)
        {
            transactions = transactions.Where(transaction => transaction.CreatedAt >= createdFrom);
        }

        if (filter.CreatedBefore is { } createdBefore)
        {
            transactions = transactions.Where(transaction => transaction.CreatedAt < createdBefore);
        }

        if (filter.Type is { } type)
        {
            transactions = transactions.Where(transaction => transaction.Type == type);
        }

        if (filter.Status is { } status)
        {
            transactions = transactions.Where(transaction => transaction.Status == status);
        }

        if (filter.MinAmount is { } minAmount)
        {
            transactions = transactions.Where(transaction => transaction.Amount >= minAmount);
        }

        if (filter.MaxAmount is { } maxAmount)
        {
            transactions = transactions.Where(transaction => transaction.Amount <= maxAmount);
        }

        var ordered = sort switch
        {
            TransactionHistorySort.CreatedAtAsc => transactions.OrderBy(transaction => transaction.CreatedAt).ThenBy(transaction => transaction.ReferenceNo),
            TransactionHistorySort.AmountDesc => transactions.OrderByDescending(transaction => transaction.Amount).ThenByDescending(transaction => transaction.ReferenceNo),
            TransactionHistorySort.AmountAsc => transactions.OrderBy(transaction => transaction.Amount).ThenBy(transaction => transaction.ReferenceNo),
            _ => transactions.OrderByDescending(transaction => transaction.CreatedAt).ThenByDescending(transaction => transaction.ReferenceNo),
        };

        return (from transaction in ordered
                let isIncoming = transaction.TargetAccountId == accountId
                let counterpartyId = isIncoming ? transaction.SourceAccountId : transaction.TargetAccountId
                select new AccountTransactionDto(
                    transaction.Id,
                    transaction.ReferenceNo,
                    transaction.Type,
                    transaction.Status,
                    isIncoming ? TransactionDirection.In : TransactionDirection.Out,
                    transaction.Amount,
                    transaction.Currency,
                    dbContext.Accounts.Where(account => account.Id == counterpartyId).Select(account => account.AccountNo).FirstOrDefault(),
                    transaction.Description,
                    transaction.FailureReason,
                    transaction.CreatedAt,
                    transaction.CompletedAt))
            .ToPagedResultAsync(page, cancellationToken);
    }
}
