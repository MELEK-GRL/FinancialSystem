using FinancialTransactions.Application.Accounts.Queries;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Localization;
using FinancialTransactions.Application.Common.Paging;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Application.Common.Time;
using FinancialTransactions.Application.Common.Validation;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Transactions;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Transactions.Queries;

public sealed record GetAccountHistoryQuery(
    Guid AccountId,
    DateOnly? From = null,
    DateOnly? To = null,
    TransactionType? Type = null,
    TransactionStatus? Status = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null) : IRequest<PagedResult<AccountTransactionDto>>;

public sealed class GetAccountHistoryQueryValidator : AbstractValidator<GetAccountHistoryQuery>
{
    public GetAccountHistoryQueryValidator()
    {
        RuleFor(query => query.Type).IsInEnum();
        RuleFor(query => query.Status).IsInEnum();

        RuleFor(query => query.MinAmount!.Value)
            .MustBeValidAmount()
            .OverridePropertyName(nameof(GetAccountHistoryQuery.MinAmount))
            .When(query => query.MinAmount.HasValue);

        RuleFor(query => query.MaxAmount!.Value)
            .MustBeValidAmount()
            .OverridePropertyName(nameof(GetAccountHistoryQuery.MaxAmount))
            .When(query => query.MaxAmount.HasValue);

        RuleFor(query => query.To)
            .GreaterThanOrEqualTo(query => query.From)
            .WithMessage(_ => Messages.Get(Messages.DateRangeInvalid))
            .When(query => query.From.HasValue && query.To.HasValue);

        RuleFor(query => query.MaxAmount)
            .GreaterThanOrEqualTo(query => query.MinAmount)
            .WithMessage(_ => Messages.Get(Messages.AmountRangeInvalid))
            .When(query => query.MinAmount.HasValue && query.MaxAmount.HasValue);
    }
}

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

        var filter = new TransactionHistoryFilter(
            query.From is { } from ? IstanbulTime.StartOfDay(from) : null,
            query.To is { } to ? IstanbulTime.StartOfDay(to.AddDays(1)) : null,
            query.Type,
            query.Status,
            query.MinAmount,
            query.MaxAmount);

        return await transactionQueries.GetAccountHistoryAsync(account.Id, filter, new PageRequest(), cancellationToken);
    }
}
