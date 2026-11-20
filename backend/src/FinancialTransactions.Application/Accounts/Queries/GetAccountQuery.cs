using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Domain.Accounts;
using MediatR;

namespace FinancialTransactions.Application.Accounts.Queries;

public sealed record GetAccountQuery(Guid AccountId) : IRequest<AccountDto>;

public sealed record GetAccountBalanceQuery(Guid AccountId) : IRequest<AccountBalanceDto>;

public sealed class GetAccountQueryHandler(ICurrentUser currentUser, IAccountQueries accountQueries)
    : IRequestHandler<GetAccountQuery, AccountDto>, IRequestHandler<GetAccountBalanceQuery, AccountBalanceDto>
{
    public Task<AccountDto> Handle(GetAccountQuery query, CancellationToken cancellationToken) =>
        GetAccessibleAccountAsync(query.AccountId, cancellationToken);

    public async Task<AccountBalanceDto> Handle(GetAccountBalanceQuery query, CancellationToken cancellationToken)
    {
        var account = await GetAccessibleAccountAsync(query.AccountId, cancellationToken);
        return new AccountBalanceDto(account.Id, account.AccountNo, account.Balance, account.Currency);
    }

    private async Task<AccountDto> GetAccessibleAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var account = await accountQueries.GetByIdAsync(accountId, cancellationToken);

        if (account is null || !currentUser.CanAccessCustomer(account.CustomerId))
        {
            throw new AccountNotFoundException();
        }

        return account;
    }
}
