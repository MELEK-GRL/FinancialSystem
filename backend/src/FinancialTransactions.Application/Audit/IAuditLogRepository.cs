using FinancialTransactions.Domain.Audit;

namespace FinancialTransactions.Application.Audit;

public interface IAuditLogRepository
{
    void Add(AuditLog auditLog);
}
