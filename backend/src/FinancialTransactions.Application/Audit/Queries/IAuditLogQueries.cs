using FinancialTransactions.Application.Common.Paging;

namespace FinancialTransactions.Application.Audit.Queries;

public interface IAuditLogQueries
{
    Task<PagedResult<AuditLogDto>> ListAsync(AuditLogFilter filter, PageRequest page, CancellationToken cancellationToken);
}
