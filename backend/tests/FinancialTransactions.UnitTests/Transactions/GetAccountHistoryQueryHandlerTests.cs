using FinancialTransactions.Application.Accounts;
using FinancialTransactions.Application.Accounts.Queries;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Paging;
using FinancialTransactions.Application.Transactions.Queries;
using FinancialTransactions.Domain.Accounts;
using NSubstitute;

namespace FinancialTransactions.UnitTests.Transactions;

public class GetAccountHistoryQueryHandlerTests
{
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly IAccountQueries accountQueries = Substitute.For<IAccountQueries>();
    private readonly ITransactionQueries transactionQueries = Substitute.For<ITransactionQueries>();
    private readonly AccountDto account = AccountDto.From(Account.Open(Guid.NewGuid()));
    private readonly GetAccountHistoryQueryHandler handler;

    public GetAccountHistoryQueryHandlerTests()
    {
        accountQueries.GetByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);
        transactionQueries.GetAccountHistoryAsync(account.Id, Arg.Any<PageRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<AccountTransactionDto>([], 1, PageRequest.DefaultSize, 0));
        handler = new GetAccountHistoryQueryHandler(currentUser, accountQueries, transactionQueries);
    }

    [Fact]
    public async Task Owner_GetsHistory()
    {
        currentUser.CustomerId.Returns(account.CustomerId);

        var result = await handler.Handle(new GetAccountHistoryQuery(account.Id), CancellationToken.None);

        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task OtherCustomer_GetsNotFound_AndHistoryIsNotQueried()
    {
        currentUser.CustomerId.Returns(Guid.NewGuid());

        await Assert.ThrowsAsync<AccountNotFoundException>(() => handler.Handle(new GetAccountHistoryQuery(account.Id), CancellationToken.None));
        await transactionQueries.DidNotReceive()
            .GetAccountHistoryAsync(Arg.Any<Guid>(), Arg.Any<PageRequest>(), Arg.Any<CancellationToken>());
    }
}
