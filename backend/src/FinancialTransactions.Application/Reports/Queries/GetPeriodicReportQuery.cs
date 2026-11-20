using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Localization;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Application.Common.Time;
using FinancialTransactions.Domain.Accounts;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Reports.Queries;

public enum ReportPeriod
{
    Daily,
    Weekly,
    Monthly,
}

public sealed record GetPeriodicReportQuery(ReportPeriod? Period, DateOnly? From = null, DateOnly? To = null)
    : IRequest<PeriodicReportDto>;

public sealed record PeriodicReportDto(
    ReportPeriod Period,
    DateOnly? From,
    DateOnly? To,
    string Currency,
    IReadOnlyList<PeriodItemDto> Items);

public sealed record PeriodItemDto(DateOnly PeriodStart, decimal TotalInflow, decimal TotalOutflow, decimal NetFlow, long TransactionCount);

public sealed class GetPeriodicReportQueryValidator : AbstractValidator<GetPeriodicReportQuery>
{
    public GetPeriodicReportQueryValidator()
    {
        RuleFor(query => query.Period).NotNull().IsInEnum();

        RuleFor(query => query.To)
            .GreaterThanOrEqualTo(query => query.From)
            .WithMessage(_ => Messages.Get(Messages.DateRangeInvalid))
            .When(query => query.From.HasValue && query.To.HasValue);
    }
}

public sealed class GetPeriodicReportQueryHandler(ICurrentUser currentUser, IReportQueries reportQueries)
    : IRequestHandler<GetPeriodicReportQuery, PeriodicReportDto>
{
    public async Task<PeriodicReportDto> Handle(GetPeriodicReportQuery query, CancellationToken cancellationToken)
    {
        var period = query.Period!.Value;
        var customerId = currentUser.IsAdmin ? (Guid?)null : currentUser.RequireCustomerId();

        var totals = await reportQueries.GetPeriodicAsync(
            period,
            query.From is { } from ? IstanbulTime.StartOfDay(from) : null,
            query.To is { } to ? IstanbulTime.StartOfDay(to.AddDays(1)) : null,
            customerId,
            cancellationToken);

        var items = totals
            .Select(total => new PeriodItemDto(
                total.PeriodStart,
                total.TotalInflow,
                total.TotalOutflow,
                total.TotalInflow - total.TotalOutflow,
                total.TransactionCount))
            .ToList();

        return new PeriodicReportDto(period, query.From, query.To, Account.DefaultCurrency, items);
    }
}
