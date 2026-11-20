using FinancialTransactions.Application.Audit.Queries;
using FinancialTransactions.Application.Common.Paging;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Queries;

public sealed class AuditLogQueries(AppDbContext dbContext) : IAuditLogQueries
{
    public Task<PagedResult<AuditLogDto>> ListAsync(AuditLogFilter filter, PageRequest page, CancellationToken cancellationToken)
    {
        var auditLogs = dbContext.AuditLogs.AsNoTracking();

        if (filter.AccountId is not null)
        {
            auditLogs = auditLogs.Where(log => log.AccountId == filter.AccountId);
        }

        if (filter.Action is not null)
        {
            auditLogs = auditLogs.Where(log => log.Action == filter.Action);
        }

        if (filter.From is not null)
        {
            auditLogs = auditLogs.Where(log => log.CreatedAt >= filter.From);
        }

        if (filter.To is not null)
        {
            auditLogs = auditLogs.Where(log => log.CreatedAt <= filter.To);
        }

        return (from log in auditLogs
                join account in dbContext.Accounts on log.AccountId equals account.Id
                join user in dbContext.Users on log.PerformedBy equals user.Id
                orderby log.CreatedAt descending, log.Id
                select new AuditLogDto(
                    log.Id,
                    log.AccountId,
                    account.AccountNo,
                    log.Action,
                    log.OldStatus,
                    log.NewStatus,
                    log.Reason,
                    log.PerformedBy,
                    user.Email,
                    log.CorrelationId,
                    log.CreatedAt))
            .ToPagedResultAsync(page, cancellationToken);
    }
}
