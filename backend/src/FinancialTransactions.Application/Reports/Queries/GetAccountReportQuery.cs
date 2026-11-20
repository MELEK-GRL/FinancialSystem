using FinancialTransactions.Application.Accounts.Queries;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Localization;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Application.Common.Time;
using FinancialTransactions.Domain.Accounts;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Reports.Queries;

public sealed record GetAccountReportQuery(Guid AccountId, DateOnly? From = null, DateOnly? To = null) : IRequest<AccountReportDto>;

public sealed record MovementDto(long Count, decimal Amount);

public sealed record AccountReportDto(
    Guid AccountId,
    string AccountNo,
    DateOnly? From,
    DateOnly? To,
    string Currency,
    decimal TotalInflow,
    decimal TotalOutflow,
    decimal NetFlow,
    MovementDto Deposits,
    MovementDto Withdrawals,
    MovementDto IncomingTransfers,
    MovementDto OutgoingTransfers);

public sealed class GetAccountReportQueryValidator : AbstractValidator<GetAccountReportQuery>
{
    public GetAccountReportQueryValidator()
    {
        RuleFor(query => query.To)
            .GreaterThanOrEqualTo(query => query.From)
            .WithMessage(_ => Messages.Get(Messages.DateRangeInvalid))
            .When(query => query.From.HasValue && query.To.HasValue);
    }
}

public sealed class GetAccountReportQueryHandler(
    ICurrentUser currentUser,
    IAccountQueries accountQueries,
    IReportQueries reportQueries) : IRequestHandler<GetAccountReportQuery, AccountReportDto>
{
    public async Task<AccountReportDto> Handle(GetAccountReportQuery query, CancellationToken cancellationToken)
    {
        var account = await accountQueries.GetByIdAsync(query.AccountId, cancellationToken);

        if (account is null || !currentUser.CanAccessCustomer(account.CustomerId))
        {
            throw new AccountNotFoundException();
        }

        var totals = await reportQueries.GetAccountMovementsAsync(
            account.Id,
            query.From is { } from ? IstanbulTime.StartOfDay(from) : null,
            query.To is { } to ? IstanbulTime.StartOfDay(to.AddDays(1)) : null,
            cancellationToken);

        var inflow = totals.DepositAmount + totals.IncomingTransferAmount;
        var outflow = totals.WithdrawalAmount + totals.OutgoingTransferAmount;

        return new AccountReportDto(
            account.Id,
            account.AccountNo,
            query.From,
            query.To,
            account.Currency,
            inflow,
            outflow,
            inflow - outflow,
            new MovementDto(totals.DepositCount, totals.DepositAmount),
            new MovementDto(totals.WithdrawalCount, totals.WithdrawalAmount),
            new MovementDto(totals.IncomingTransferCount, totals.IncomingTransferAmount),
            new MovementDto(totals.OutgoingTransferCount, totals.OutgoingTransferAmount));
    }
}
