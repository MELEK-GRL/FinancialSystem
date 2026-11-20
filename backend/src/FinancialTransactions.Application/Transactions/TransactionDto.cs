using FinancialTransactions.Domain.Transactions;

namespace FinancialTransactions.Application.Transactions;

public sealed record TransactionDto(
    Guid Id,
    string ReferenceNo,
    TransactionType Type,
    TransactionStatus Status,
    decimal Amount,
    string Currency,
    Guid? SourceAccountId,
    Guid? TargetAccountId,
    string? Description,
    string? FailureReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt)
{
    public static TransactionDto From(Transaction transaction) =>
        new(
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
            transaction.CompletedAt);
}
