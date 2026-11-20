using FinancialTransactions.Application.Reports.Queries;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Queries;

public sealed class ReportQueries(AppDbContext dbContext) : IReportQueries
{
    public Task<SummaryTotals> GetSummaryAsync(DateTimeOffset? createdFrom, DateTimeOffset? createdBefore, CancellationToken cancellationToken) =>
        dbContext.Database
            .SqlQuery<SummaryTotals>($"""
                SELECT
                    COALESCE(SUM(amount) FILTER (WHERE type = 'Deposit'), 0) AS total_inflow,
                    COALESCE(SUM(amount) FILTER (WHERE type = 'Withdrawal'), 0) AS total_outflow,
                    COUNT(*) FILTER (WHERE type = 'Deposit') AS deposit_count,
                    COUNT(*) FILTER (WHERE type = 'Withdrawal') AS withdrawal_count
                FROM transactions
                WHERE status = 'Success'
                  AND type IN ('Deposit', 'Withdrawal')
                  AND ({createdFrom}::timestamptz IS NULL OR created_at >= {createdFrom})
                  AND ({createdBefore}::timestamptz IS NULL OR created_at < {createdBefore})
                """)
            .SingleAsync(cancellationToken);
}
