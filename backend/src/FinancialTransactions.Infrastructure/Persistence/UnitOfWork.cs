using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres)
        {
            dbContext.ChangeTracker.Clear();
            throw new UniqueConstraintViolationException(postgres.ConstraintName, exception);
        }
        catch
        {
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }
}
