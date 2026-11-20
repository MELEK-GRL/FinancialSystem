using FinancialTransactions.Domain.Accounts;

namespace FinancialTransactions.Domain.Transactions;

public sealed class Transaction
{
    private Transaction()
    {
    }

    public Guid Id { get; private set; }

    public string ReferenceNo { get; private set; } = string.Empty;

    public TransactionType Type { get; private set; }

    public TransactionStatus Status { get; private set; }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = Account.DefaultCurrency;

    public Guid? SourceAccountId { get; private set; }

    public Guid? TargetAccountId { get; private set; }

    public string? Description { get; private set; }

    public string? FailureReason { get; private set; }

    public string? IdempotencyKey { get; private set; }

    public Guid InitiatedByCustomerId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public static Transaction CreateDeposit(
        string referenceNo, Guid targetAccountId, decimal amount, string? description, Guid customerId, string? idempotencyKey) =>
        Create(referenceNo, TransactionType.Deposit, amount, null, targetAccountId, description, customerId, idempotencyKey);

    public static Transaction CreateWithdrawal(
        string referenceNo, Guid sourceAccountId, decimal amount, string? description, Guid customerId, string? idempotencyKey) =>
        Create(referenceNo, TransactionType.Withdrawal, amount, sourceAccountId, null, description, customerId, idempotencyKey);

    public static Transaction CreateTransfer(
        string referenceNo, Guid sourceAccountId, Guid targetAccountId, decimal amount, string? description, Guid customerId, string? idempotencyKey) =>
        Create(referenceNo, TransactionType.Transfer, amount, sourceAccountId, targetAccountId, description, customerId, idempotencyKey);

    public bool IsSameRequestAs(Transaction other) =>
        Type == other.Type
        && SourceAccountId == other.SourceAccountId
        && TargetAccountId == other.TargetAccountId
        && Amount == other.Amount
        && Description == other.Description;

    public void MarkSucceeded()
    {
        EnsurePending();
        Status = TransactionStatus.Success;
    }

    public void MarkFailed(string reason)
    {
        EnsurePending();
        Status = TransactionStatus.Failed;
        FailureReason = reason;
    }

    private void EnsurePending()
    {
        if (Status != TransactionStatus.Pending)
        {
            throw new InvalidOperationException($"Transaction {ReferenceNo} is already {Status}");
        }
    }

    private static Transaction Create(
        string referenceNo,
        TransactionType type,
        decimal amount,
        Guid? sourceAccountId,
        Guid? targetAccountId,
        string? description,
        Guid customerId,
        string? idempotencyKey)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            ReferenceNo = referenceNo,
            Type = type,
            Status = TransactionStatus.Pending,
            Amount = amount,
            Currency = Account.DefaultCurrency,
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            InitiatedByCustomerId = customerId,
            IdempotencyKey = idempotencyKey,
        };
    }
}
