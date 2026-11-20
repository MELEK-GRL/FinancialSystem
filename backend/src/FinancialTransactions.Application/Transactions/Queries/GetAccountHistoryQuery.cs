using FinancialTransactions.Application.Accounts.Queries;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Paging;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Domain.Accounts;
using MediatR;

namespace FinancialTransactions.Application.Transactions.Queries;

public sealed record GetAccountHistoryQuery(Guid AccountId) : IRequest<PagedResult<AccountTransactionDto>>;

public sealed class GetAccountHistoryQueryHandler(
    ICurrentUser currentUser,
    IAccountQueries accountQueries,
    ITransactionQueries transactionQueries) : IRequestHandler<GetAccountHistoryQuery, PagedResult<AccountTransactionDto>>
{
    public async Task<PagedResult<AccountTransactionDto>> Handle(GetAccountHistoryQuery query, CancellationToken cancellationToken)
    {
        var account = await accountQueries.GetByIdAsync(query.AccountId, cancellationToken);

        if (account is null || !currentUser.CanAccessCustomer(account.CustomerId))
        {
            throw new AccountNotFoundException();
        }

        return await transactionQueries.GetAccountHistoryAsync(account.Id, new PageRequest(), cancellationToken);
    }
}
