namespace FinancialTransactions.Contracts.Transactions;

public static class TransactionEventTypes
{
    public const string Completed = "transaction.completed";
    public const string Failed = "transaction.failed";
}

public sealed record TransactionEvent(
    Guid TransactionId,
    string ReferenceNo,
    string Type,
    string Status,
    decimal Amount,
    string Currency,
    Guid? SourceAccountId,
    Guid? SourceCustomerId,
    Guid? TargetAccountId,
    Guid? TargetCustomerId,
    string? FailureReason);
