using FinancialTransactions.Domain.Accounts;

namespace FinancialTransactions.Domain.Audit;

public sealed class AuditLog
{
    private AuditLog()
    {
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public AuditAction Action { get; private set; }

    public AccountStatus OldStatus { get; private set; }

    public AccountStatus NewStatus { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public Guid PerformedBy { get; private set; }

    public string CorrelationId { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static AuditLog ForAccountStatusChange(
        Guid accountId,
        AccountStatus oldStatus,
        AccountStatus newStatus,
        string reason,
        Guid performedBy,
        string correlationId) =>
        new()
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Action = newStatus == AccountStatus.Active ? AuditAction.AccountActivated : AuditAction.AccountDeactivated,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Reason = reason.Trim(),
            PerformedBy = performedBy,
            CorrelationId = correlationId,
        };
}

public enum AuditAction
{
    AccountActivated,
    AccountDeactivated,
}
