using FinancialTransactions.Application.Accounts;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Options;
using FinancialTransactions.Application.Customers;
using FinancialTransactions.Application.Transactions;
using FinancialTransactions.Contracts.Transactions;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.Domain.Transactions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FinancialTransactions.UnitTests.Transactions;

public class TransactionExecutorTests
{
    private readonly IAccountRepository accounts = Substitute.For<IAccountRepository>();
    private readonly ICustomerRepository customers = Substitute.For<ICustomerRepository>();
    private readonly ITransactionRepository transactions = Substitute.For<ITransactionRepository>();
    private readonly IOutbox outbox = Substitute.For<IOutbox>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly Transaction transaction = Transaction.CreateDeposit("TX1", Guid.NewGuid(), 10m, null, Guid.NewGuid(), null);
    private readonly TransactionExecutor executor;

    public TransactionExecutorTests()
    {
        unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task>>()(call.Arg<CancellationToken>()));
        transactions.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);
        accounts.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns(Array.Empty<Account>());

        executor = new TransactionExecutor(
            accounts,
            customers,
            transactions,
            outbox,
            unitOfWork,
            Options.Create(new TransactionOptions { MaxAmount = 1000m }),
            NullLogger<TransactionExecutor>.Instance);
    }

    [Fact]
    public async Task UnexpectedError_MarksTransactionFailed_AndRethrows()
    {
        accounts.GetForUpdateAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection lost"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => executor.ExecuteAsync(transaction, _ => { }, CancellationToken.None));

        Assert.Equal(TransactionStatus.Failed, transaction.Status);
        Assert.Equal(ErrorCodes.ProcessingError, transaction.FailureReason);
        outbox.Received(1).Enqueue(TransactionEventTypes.Failed, transaction.Id, Arg.Any<TransactionEvent>());
    }

    [Fact]
    public async Task FailureThatCannotBeRecorded_DoesNotHideOriginalError()
    {
        accounts.GetForUpdateAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection lost"));
        transactions.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).ThrowsAsync(new TimeoutException());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => executor.ExecuteAsync(transaction, _ => { }, CancellationToken.None));

        Assert.Equal("connection lost", exception.Message);
    }

    [Fact]
    public async Task MoneyMovement_IgnoresRequestCancellation()
    {
        var account = Account.Open(Guid.NewGuid());
        var deposit = Transaction.CreateDeposit("TX2", account.Id, 10m, null, account.CustomerId, null);
        accounts.GetForUpdateAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns([account]);
        customers.AllActiveAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns(true);
        using var cancellation = new CancellationTokenSource();

        await executor.ExecuteAsync(
            deposit,
            locked =>
            {
                cancellation.Cancel();
                locked[account.Id].Credit(deposit.Amount);
            },
            cancellation.Token);

        Assert.Equal(TransactionStatus.Success, deposit.Status);
        Assert.Equal(10m, account.Balance);
        await accounts.Received(1).GetForUpdateAsync(Arg.Any<IReadOnlyCollection<Guid>>(), CancellationToken.None);
    }
}
