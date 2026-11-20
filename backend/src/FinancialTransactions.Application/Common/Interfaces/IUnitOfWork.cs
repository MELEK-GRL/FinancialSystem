namespace FinancialTransactions.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);

    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken);
}
