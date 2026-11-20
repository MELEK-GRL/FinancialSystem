namespace FinancialTransactions.Application.Reports.Queries;

public interface IReportQueries
{
    Task<SummaryTotals> GetSummaryAsync(DateTimeOffset? createdFrom, DateTimeOffset? createdBefore, CancellationToken cancellationToken);
}

public sealed record SummaryTotals(decimal TotalInflow, decimal TotalOutflow, long DepositCount, long WithdrawalCount);
