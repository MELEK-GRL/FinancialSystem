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
            ? CustomerPeriodicQuery(unit, createdFrom, createdBefore, await OwnAccountIdsAsync(customer, cancellationToken))
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

    public Task<AccountMovementTotals> GetAccountMovementsAsync(
        Guid accountId,
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdBefore,
        CancellationToken cancellationToken) =>
        dbContext.Database
            .SqlQuery<AccountMovementTotals>($"""
                SELECT
                    COUNT(*) FILTER (WHERE type = 'Deposit') AS deposit_count,
                    COALESCE(SUM(amount) FILTER (WHERE type = 'Deposit'), 0) AS deposit_amount,
                    COUNT(*) FILTER (WHERE type = 'Withdrawal') AS withdrawal_count,
                    COALESCE(SUM(amount) FILTER (WHERE type = 'Withdrawal'), 0) AS withdrawal_amount,
                    COUNT(*) FILTER (WHERE type = 'Transfer' AND target_account_id = {accountId}) AS incoming_transfer_count,
                    COALESCE(SUM(amount) FILTER (WHERE type = 'Transfer' AND target_account_id = {accountId}), 0) AS incoming_transfer_amount,
                    COUNT(*) FILTER (WHERE type = 'Transfer' AND source_account_id = {accountId}) AS outgoing_transfer_count,
                    COALESCE(SUM(amount) FILTER (WHERE type = 'Transfer' AND source_account_id = {accountId}), 0) AS outgoing_transfer_amount
                FROM transactions
                WHERE status = 'Success'
                  AND (source_account_id = {accountId} OR target_account_id = {accountId})
                  AND ({createdFrom}::timestamptz IS NULL OR created_at >= {createdFrom})
                  AND ({createdBefore}::timestamptz IS NULL OR created_at < {createdBefore})
                """)
            .SingleAsync(cancellationToken);

    public async Task<IReadOnlyList<TypeStatusCounts>> GetStatusCountsByTypeAsync(
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdBefore,
        Guid? initiatedByCustomerId,
        CancellationToken cancellationToken) =>
        await dbContext.Database
            .SqlQuery<TypeStatusCounts>($"""
                SELECT
                    type,
                    COUNT(*) AS total_count,
                    COUNT(*) FILTER (WHERE status = 'Success') AS success_count,
                    COUNT(*) FILTER (WHERE status = 'Failed') AS failed_count,
                    COUNT(*) FILTER (WHERE status = 'Pending') AS pending_count
                FROM transactions
                WHERE ({initiatedByCustomerId}::uuid IS NULL OR initiated_by_customer_id = {initiatedByCustomerId})
                  AND ({createdFrom}::timestamptz IS NULL OR created_at >= {createdFrom})
                  AND ({createdBefore}::timestamptz IS NULL OR created_at < {createdBefore})
                GROUP BY type
                """)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<FailureReasonCount>> GetFailureReasonsAsync(
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdBefore,
        Guid? initiatedByCustomerId,
        CancellationToken cancellationToken) =>
        await dbContext.Database
            .SqlQuery<FailureReasonCount>($"""
                SELECT failure_reason AS reason, COUNT(*) AS count
                FROM transactions
                WHERE status = 'Failed'
                  AND ({initiatedByCustomerId}::uuid IS NULL OR initiated_by_customer_id = {initiatedByCustomerId})
                  AND ({createdFrom}::timestamptz IS NULL OR created_at >= {createdFrom})
                  AND ({createdBefore}::timestamptz IS NULL OR created_at < {createdBefore})
                GROUP BY failure_reason
                """)
            .OrderByDescending(reason => reason.Count)
            .ThenBy(reason => reason.Reason)
            .ToListAsync(cancellationToken);

    private IQueryable<PeriodTotals> CustomerPeriodicQuery(
        string unit,
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdBefore,
        Guid[] ownAccounts) =>
        dbContext.Database.SqlQuery<PeriodTotals>($"""
            WITH flows AS (
                SELECT
                    created_at,
                    CASE WHEN target_account_id = ANY({ownAccounts}) AND NOT COALESCE(source_account_id = ANY({ownAccounts}), false)
                         THEN amount ELSE 0 END AS inflow,
                    CASE WHEN source_account_id = ANY({ownAccounts}) AND NOT COALESCE(target_account_id = ANY({ownAccounts}), false)
                         THEN amount ELSE 0 END AS outflow
                FROM transactions
                WHERE status = 'Success'
                  AND (source_account_id = ANY({ownAccounts}) OR target_account_id = ANY({ownAccounts}))
                  AND ({createdFrom}::timestamptz IS NULL OR created_at >= {createdFrom})
                  AND ({createdBefore}::timestamptz IS NULL OR created_at < {createdBefore})
            )
            SELECT
                date_trunc({unit}, created_at AT TIME ZONE 'Europe/Istanbul')::date AS period_start,
                SUM(inflow) AS total_inflow,
                SUM(outflow) AS total_outflow,
                COUNT(*) AS transaction_count
            FROM flows
            WHERE inflow > 0 OR outflow > 0
            GROUP BY 1
            """);

    private Task<Guid[]> OwnAccountIdsAsync(Guid customerId, CancellationToken cancellationToken) =>
        dbContext.Accounts
            .Where(account => account.CustomerId == customerId)
            .Select(account => account.Id)
            .ToArrayAsync(cancellationToken);
}
