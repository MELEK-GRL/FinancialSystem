using FinancialTransactions.Application.Common.Localization;
using FinancialTransactions.Application.Common.Time;
using FinancialTransactions.Domain.Accounts;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Reports.Queries;

public sealed record GetSummaryQuery(DateOnly? From = null, DateOnly? To = null) : IRequest<SummaryReportDto>;

public sealed record SummaryReportDto(
    DateOnly? From,
    DateOnly? To,
    string Currency,
    decimal TotalInflow,
    decimal TotalOutflow,
    decimal NetFlow,
    long DepositCount,
    long WithdrawalCount);

public sealed class GetSummaryQueryValidator : AbstractValidator<GetSummaryQuery>
{
    public GetSummaryQueryValidator()
    {
        RuleFor(query => query.To)
            .GreaterThanOrEqualTo(query => query.From)
            .WithMessage(_ => Messages.Get(Messages.DateRangeInvalid))
            .When(query => query.From.HasValue && query.To.HasValue);
    }
}

public sealed class GetSummaryQueryHandler(IReportQueries reportQueries) : IRequestHandler<GetSummaryQuery, SummaryReportDto>
{
    public async Task<SummaryReportDto> Handle(GetSummaryQuery query, CancellationToken cancellationToken)
    {
        var totals = await reportQueries.GetSummaryAsync(
            query.From is { } from ? IstanbulTime.StartOfDay(from) : null,
            query.To is { } to ? IstanbulTime.StartOfDay(to.AddDays(1)) : null,
            cancellationToken);

        return new SummaryReportDto(
            query.From,
            query.To,
            Account.DefaultCurrency,
            totals.TotalInflow,
            totals.TotalOutflow,
            totals.TotalInflow - totals.TotalOutflow,
            totals.DepositCount,
            totals.WithdrawalCount);
    }
}
