using FinancialTransactions.Application.Accounts;
using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Options;
using FinancialTransactions.Application.Customers;
using FinancialTransactions.Contracts.Transactions;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Transactions;
using Microsoft.Extensions.Options;

namespace FinancialTransactions.Application.Transactions;

public sealed class TransactionExecutor(
    IAccountRepository accounts,
    ICustomerRepository customers,
    ITransactionRepository transactions,
    IOutbox outbox,
    IUnitOfWork unitOfWork,
    IOptions<TransactionOptions> options)
{
    public async Task<TransactionDto> ExecuteAsync(
        Transaction transaction,
        Action<IReadOnlyDictionary<Guid, Account>> moveMoney,
        CancellationToken cancellationToken)
    {
        var previous = await FindPreviousAsync(transaction, cancellationToken);

        if (previous is not null)
        {
            return Replay(previous, transaction);
        }

        try
        {
            await unitOfWork.ExecuteInTransactionAsync(_ =>
            {
                transactions.Add(transaction);
                return Task.CompletedTask;
            }, cancellationToken);
        }
        catch (UniqueConstraintViolationException) when (transaction.IdempotencyKey is not null)
        {
            previous = await FindPreviousAsync(transaction, cancellationToken);

            if (previous is null)
            {
                throw;
            }

            return Replay(previous, transaction);
        }

        try
        {
            await unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                var lockedAccounts = await accounts.GetForUpdateAsync(AccountIdsOf(transaction), token);
                await EnsureAllowedAsync(transaction, lockedAccounts, token);

                moveMoney(lockedAccounts.ToDictionary(account => account.Id));
                transaction.MarkSucceeded();
                outbox.Enqueue(TransactionEventTypes.Completed, transaction.Id, ToEvent(transaction, lockedAccounts));
            }, cancellationToken);
        }
        catch (BusinessRuleException exception)
        {
            await RecordFailureAsync(transaction.Id, exception.ErrorCode, cancellationToken);
            throw;
        }

        return TransactionDto.From(transaction);
    }

    private async Task<Transaction?> FindPreviousAsync(Transaction transaction, CancellationToken cancellationToken) =>
        transaction.IdempotencyKey is { } key
            ? await transactions.GetByIdempotencyKeyAsync(transaction.InitiatedByCustomerId, key, cancellationToken)
            : null;

    private static TransactionDto Replay(Transaction previous, Transaction request)
    {
        if (!previous.IsSameRequestAs(request))
        {
            throw new IdempotencyConflictException();
        }

        return previous.Status switch
        {
            TransactionStatus.Success => TransactionDto.From(previous),
            TransactionStatus.Failed => throw new TransactionFailedException(previous.FailureReason ?? ErrorCodes.InternalError),
            _ => throw new IdempotencyInProgressException(),
        };
    }

    private async Task EnsureAllowedAsync(Transaction transaction, IReadOnlyList<Account> lockedAccounts, CancellationToken cancellationToken)
    {
        if (transaction.Amount > options.Value.MaxAmount)
        {
            throw new AmountLimitExceededException();
        }

        foreach (var account in lockedAccounts)
        {
            account.EnsureActive();
        }

        var ownerIds = lockedAccounts.Select(account => account.CustomerId).Distinct().ToArray();

        if (!await customers.AllActiveAsync(ownerIds, cancellationToken))
        {
            throw new CustomerInactiveException();
        }
    }

    private async Task RecordFailureAsync(Guid transactionId, string reason, CancellationToken cancellationToken) =>
        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var failed = await transactions.GetByIdAsync(transactionId, token)
                ?? throw new InvalidOperationException($"Transaction {transactionId} was not found");
            var involvedAccounts = await accounts.GetByIdsAsync(AccountIdsOf(failed), token);

            failed.MarkFailed(reason);
            outbox.Enqueue(TransactionEventTypes.Failed, failed.Id, ToEvent(failed, involvedAccounts));
        }, cancellationToken);

    private static Guid[] AccountIdsOf(Transaction transaction) =>
        new[] { transaction.SourceAccountId, transaction.TargetAccountId }.OfType<Guid>().ToArray();

    private static TransactionEvent ToEvent(Transaction transaction, IEnumerable<Account> involvedAccounts)
    {
        var owners = involvedAccounts.ToDictionary(account => account.Id, account => account.CustomerId);
        Guid? OwnerOf(Guid? accountId) => accountId is { } id && owners.TryGetValue(id, out var owner) ? owner : null;

        return new TransactionEvent(
            transaction.Id,
            transaction.ReferenceNo,
            transaction.Type.ToString(),
            transaction.Status.ToString(),
            transaction.Amount,
            transaction.Currency,
            transaction.SourceAccountId,
            OwnerOf(transaction.SourceAccountId),
            transaction.TargetAccountId,
            OwnerOf(transaction.TargetAccountId),
            transaction.FailureReason);
    }
}
