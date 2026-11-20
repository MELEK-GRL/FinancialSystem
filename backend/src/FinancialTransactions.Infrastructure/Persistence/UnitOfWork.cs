using FinancialTransactions.Application.Common.Interfaces;

namespace FinancialTransactions.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken) =>
        ExecuteInTransactionAsync(async token =>
        {
            await operation(token);
            return true;
        }, cancellationToken);

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }
}
