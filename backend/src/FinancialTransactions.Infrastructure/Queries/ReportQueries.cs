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

    public async Task<IReadOnlyList<PeriodTotals>> GetPeriodicAsync(
        ReportPeriod period,
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdBefore,
        Guid? customerId,
        CancellationToken cancellationToken)
    {
        var unit = period switch
        {
            ReportPeriod.Weekly => "week",
            ReportPeriod.Monthly => "month",
            _ => "day",
        };

        var query = customerId is { } customer
            ? dbContext.Database.SqlQuery<PeriodTotals>($"""
                WITH flows AS (
                    SELECT
                        t.created_at,
                        CASE WHEN target.customer_id = {customer} AND source.customer_id IS DISTINCT FROM {customer} THEN t.amount ELSE 0 END AS inflow,
                        CASE WHEN source.customer_id = {customer} AND target.customer_id IS DISTINCT FROM {customer} THEN t.amount ELSE 0 END AS outflow
                    FROM transactions t
                    LEFT JOIN accounts source ON source.id = t.source_account_id
                    LEFT JOIN accounts target ON target.id = t.target_account_id
                    WHERE t.status = 'Success'
                      AND (source.customer_id = {customer} OR target.customer_id = {customer})
                      AND ({createdFrom}::timestamptz IS NULL OR t.created_at >= {createdFrom})
                      AND ({createdBefore}::timestamptz IS NULL OR t.created_at < {createdBefore})
                )
                SELECT
                    date_trunc({unit}, created_at AT TIME ZONE 'Europe/Istanbul')::date AS period_start,
                    SUM(inflow) AS total_inflow,
                    SUM(outflow) AS total_outflow,
                    COUNT(*) AS transaction_count
                FROM flows
                WHERE inflow > 0 OR outflow > 0
                GROUP BY 1
                """)
            : dbContext.Database.SqlQuery<PeriodTotals>($"""
                SELECT
                    date_trunc({unit}, created_at AT TIME ZONE 'Europe/Istanbul')::date AS period_start,
                    COALESCE(SUM(amount) FILTER (WHERE type = 'Deposit'), 0) AS total_inflow,
                    COALESCE(SUM(amount) FILTER (WHERE type = 'Withdrawal'), 0) AS total_outflow,
                    COUNT(*) AS transaction_count
                FROM transactions
                WHERE status = 'Success'
                  AND type IN ('Deposit', 'Withdrawal')
                  AND ({createdFrom}::timestamptz IS NULL OR created_at >= {createdFrom})
                  AND ({createdBefore}::timestamptz IS NULL OR created_at < {createdBefore})
                GROUP BY 1
                """);

        return await query.OrderBy(total => total.PeriodStart).ToListAsync(cancellationToken);
    }
}
