using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Audit;

namespace FinancialTransactions.Application.Audit.Queries;

public sealed record AuditLogDto(
    Guid Id,
    Guid AccountId,
    string AccountNo,
    AuditAction Action,
    AccountStatus OldStatus,
    AccountStatus NewStatus,
    string Reason,
    Guid PerformedBy,
    string PerformedByEmail,
    string CorrelationId,
    DateTimeOffset CreatedAt);

public sealed record AuditLogFilter(Guid? AccountId, AuditAction? Action, DateTimeOffset? From, DateTimeOffset? To);
