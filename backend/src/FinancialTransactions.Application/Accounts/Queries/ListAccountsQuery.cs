using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Accounts.Queries;

public sealed record ListAccountsQuery(Guid? CustomerId) : IRequest<IReadOnlyList<AccountDto>>;

public sealed class ListAccountsQueryValidator : AbstractValidator<ListAccountsQuery>
{
    public ListAccountsQueryValidator(ICurrentUser currentUser)
    {
        RuleFor(query => query.CustomerId).NotEmpty().When(_ => currentUser.IsAdmin);
    }
}

public sealed class ListAccountsQueryHandler(ICurrentUser currentUser, IAccountQueries accountQueries)
    : IRequestHandler<ListAccountsQuery, IReadOnlyList<AccountDto>>
{
    public Task<IReadOnlyList<AccountDto>> Handle(ListAccountsQuery query, CancellationToken cancellationToken)
    {
        var customerId = currentUser.IsAdmin ? query.CustomerId!.Value : currentUser.RequireCustomerId();

        return accountQueries.ListByCustomerAsync(customerId, cancellationToken);
    }
}
