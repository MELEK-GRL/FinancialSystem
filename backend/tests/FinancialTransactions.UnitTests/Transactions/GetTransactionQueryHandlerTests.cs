using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Transactions;
using FinancialTransactions.Application.Transactions.Queries;
using FinancialTransactions.Domain.Transactions;
using NSubstitute;

namespace FinancialTransactions.UnitTests.Transactions;

public class GetTransactionQueryHandlerTests
{
    private const string ReferenceNo = "TX2026112000000001";
    private static readonly Guid Initiator = Guid.NewGuid();
    private static readonly Guid Receiver = Guid.NewGuid();

    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly ITransactionQueries transactionQueries = Substitute.For<ITransactionQueries>();
    private readonly GetTransactionQueryHandler handler;

    public GetTransactionQueryHandlerTests()
    {
        var transfer = Transaction.CreateTransfer(ReferenceNo, Guid.NewGuid(), Guid.NewGuid(), 10m, null, Initiator, null);
        transfer.MarkSucceeded();
        transactionQueries.GetByReferenceNoAsync(ReferenceNo, Arg.Any<CancellationToken>())
            .Returns(new TransactionWithOwners(TransactionDto.From(transfer), Initiator, Initiator, Receiver));
        handler = new GetTransactionQueryHandler(currentUser, transactionQueries);
    }

    [Fact]
    public async Task Receiver_CanSeeTransfer_AndReferenceIsNormalized()
    {
        currentUser.CustomerId.Returns(Receiver);

        var result = await handler.Handle(new GetTransactionQuery($" {ReferenceNo.ToLowerInvariant()} "), CancellationToken.None);

        Assert.Equal(ReferenceNo, result.ReferenceNo);
    }

    [Fact]
    public async Task Admin_CanSeeAnyTransaction()
    {
        currentUser.IsAdmin.Returns(true);

        Assert.NotNull(await handler.Handle(new GetTransactionQuery(ReferenceNo), CancellationToken.None));
    }

    [Fact]
    public async Task UninvolvedCustomer_GetsNotFound()
    {
        currentUser.CustomerId.Returns(Guid.NewGuid());

        await Assert.ThrowsAsync<TransactionNotFoundException>(() => handler.Handle(new GetTransactionQuery(ReferenceNo), CancellationToken.None));
    }

    [Fact]
    public async Task Receiver_CannotSeeFailedTransfer_ButSenderCan()
    {
        var failed = Transaction.CreateTransfer("TX2026112000000002", Guid.NewGuid(), Guid.NewGuid(), 10m, null, Initiator, null);
        failed.MarkFailed("INSUFFICIENT_BALANCE");
        transactionQueries.GetByReferenceNoAsync(failed.ReferenceNo, Arg.Any<CancellationToken>())
            .Returns(new TransactionWithOwners(TransactionDto.From(failed), Initiator, Initiator, Receiver));

        currentUser.CustomerId.Returns(Receiver);
        await Assert.ThrowsAsync<TransactionNotFoundException>(() => handler.Handle(new GetTransactionQuery(failed.ReferenceNo), CancellationToken.None));

        currentUser.CustomerId.Returns(Initiator);
        Assert.Equal("INSUFFICIENT_BALANCE", (await handler.Handle(new GetTransactionQuery(failed.ReferenceNo), CancellationToken.None)).FailureReason);
    }
}
