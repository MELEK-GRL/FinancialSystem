using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Localization;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Application.Common.Time;
using FinancialTransactions.Domain.Transactions;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Reports.Queries;

public sealed record GetStatisticsQuery(DateOnly? From = null, DateOnly? To = null) : IRequest<StatisticsDto>;

public sealed record StatisticsDto(
    DateOnly? From,
    DateOnly? To,
    long TotalCount,
    long SuccessCount,
    long FailedCount,
    long PendingCount,
    decimal SuccessRate,
    IReadOnlyList<TypeStatisticsDto> ByType,
    IReadOnlyList<FailureReasonDto> FailureReasons);

public sealed record TypeStatisticsDto(TransactionType Type, long TotalCount, long SuccessCount, long FailedCount, long PendingCount);

public sealed record FailureReasonDto(string Reason, string Message, long Count);

public sealed class GetStatisticsQueryValidator : AbstractValidator<GetStatisticsQuery>
{
    public GetStatisticsQueryValidator()
    {
        RuleFor(query => query.To)
            .GreaterThanOrEqualTo(query => query.From)
            .WithMessage(_ => Messages.Get(Messages.DateRangeInvalid))
            .When(query => query.From.HasValue && query.To.HasValue);
    }
}

public sealed class GetStatisticsQueryHandler(ICurrentUser currentUser, IReportQueries reportQueries)
    : IRequestHandler<GetStatisticsQuery, StatisticsDto>
{
    public async Task<StatisticsDto> Handle(GetStatisticsQuery query, CancellationToken cancellationToken)
    {
        var customerId = currentUser.IsAdmin ? (Guid?)null : currentUser.RequireCustomerId();
        var createdFrom = query.From is { } from ? IstanbulTime.StartOfDay(from) : (DateTimeOffset?)null;
        var createdBefore = query.To is { } to ? IstanbulTime.StartOfDay(to.AddDays(1)) : (DateTimeOffset?)null;

        var counts = await reportQueries.GetStatusCountsByTypeAsync(createdFrom, createdBefore, customerId, cancellationToken);
        var reasons = await reportQueries.GetFailureReasonsAsync(createdFrom, createdBefore, customerId, cancellationToken);

        var byType = Enum.GetValues<TransactionType>()
            .Select(type => counts.SingleOrDefault(count => count.Type == type.ToString()) is { } found
                ? new TypeStatisticsDto(type, found.TotalCount, found.SuccessCount, found.FailedCount, found.PendingCount)
                : new TypeStatisticsDto(type, 0, 0, 0, 0))
            .ToList();

        var success = byType.Sum(type => type.SuccessCount);
        var failed = byType.Sum(type => type.FailedCount);
        var finished = success + failed;

        return new StatisticsDto(
            query.From,
            query.To,
            byType.Sum(type => type.TotalCount),
            success,
            failed,
            byType.Sum(type => type.PendingCount),
            finished == 0 ? 0 : Math.Round(success * 100m / finished, 2),
            byType,
            reasons.Select(reason => new FailureReasonDto(reason.Reason, Messages.Get(reason.Reason), reason.Count)).ToList());
    }
}
