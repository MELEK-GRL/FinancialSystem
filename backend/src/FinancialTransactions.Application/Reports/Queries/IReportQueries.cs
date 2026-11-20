namespace FinancialTransactions.Application.Reports.Queries;

public interface IReportQueries
{
    Task<SummaryTotals> GetSummaryAsync(DateTimeOffset? createdFrom, DateTimeOffset? createdBefore, CancellationToken cancellationToken);

    Task<IReadOnlyList<PeriodTotals>> GetPeriodicAsync(
        ReportPeriod period,
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdBefore,
        Guid? customerId,
        CancellationToken cancellationToken);

    Task<AccountMovementTotals> GetAccountMovementsAsync(
        Guid accountId,
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdBefore,
        CancellationToken cancellationToken);
}

public sealed record SummaryTotals(decimal TotalInflow, decimal TotalOutflow, long DepositCount, long WithdrawalCount);

public sealed record PeriodTotals(DateOnly PeriodStart, decimal TotalInflow, decimal TotalOutflow, long TransactionCount);

public sealed record AccountMovementTotals(
    long DepositCount,
    decimal DepositAmount,
    long WithdrawalCount,
    decimal WithdrawalAmount,
    long IncomingTransferCount,
    decimal IncomingTransferAmount,
    long OutgoingTransferCount,
    decimal OutgoingTransferAmount);
