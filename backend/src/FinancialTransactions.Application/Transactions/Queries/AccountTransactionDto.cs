using FinancialTransactions.Domain.Transactions;

namespace FinancialTransactions.Application.Transactions.Queries;

public enum TransactionDirection
{
    In,
    Out,
}

public sealed record AccountTransactionDto(
    Guid Id,
    string ReferenceNo,
    TransactionType Type,
    TransactionStatus Status,
    TransactionDirection Direction,
    decimal Amount,
    string Currency,
    string? CounterpartyAccountNo,
    string? Description,
    string? FailureReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
