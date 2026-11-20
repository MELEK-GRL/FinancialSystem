using FinancialTransactions.Domain.Transactions;

namespace FinancialTransactions.Application.Transactions.Queries;

public sealed record TransactionHistoryFilter(
    DateTimeOffset? CreatedFrom = null,
    DateTimeOffset? CreatedBefore = null,
    TransactionType? Type = null,
    TransactionStatus? Status = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null);
