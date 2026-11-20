using FinancialTransactions.Application.Audit;
using FinancialTransactions.Domain.Audit;
using FinancialTransactions.Infrastructure.Persistence;

namespace FinancialTransactions.Infrastructure.Repositories;

public sealed class AuditLogRepository(AppDbContext dbContext) : IAuditLogRepository
{
    public void Add(AuditLog auditLog) => dbContext.AuditLogs.Add(auditLog);
}
