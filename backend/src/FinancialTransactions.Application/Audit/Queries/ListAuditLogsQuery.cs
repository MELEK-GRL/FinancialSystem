using FinancialTransactions.Application.Common.Localization;
using FinancialTransactions.Application.Common.Paging;
using FinancialTransactions.Domain.Audit;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Audit.Queries;

public sealed record ListAuditLogsQuery(
    Guid? AccountId,
    AuditAction? Action,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int Size) : IRequest<PagedResult<AuditLogDto>>;

public sealed class ListAuditLogsQueryValidator : AbstractValidator<ListAuditLogsQuery>
{
    public ListAuditLogsQueryValidator()
    {
        RuleFor(query => query.Action).IsInEnum();
        RuleFor(query => query.To)
            .GreaterThanOrEqualTo(query => query.From)
            .WithMessage(_ => Messages.Get(Messages.DateRangeInvalid))
            .When(query => query.From is not null && query.To is not null);
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.Size).InclusiveBetween(1, PageRequest.MaxSize);
    }
}

public sealed class ListAuditLogsQueryHandler(IAuditLogQueries auditLogQueries)
    : IRequestHandler<ListAuditLogsQuery, PagedResult<AuditLogDto>>
{
    public Task<PagedResult<AuditLogDto>> Handle(ListAuditLogsQuery query, CancellationToken cancellationToken) =>
        auditLogQueries.ListAsync(
            new AuditLogFilter(query.AccountId, query.Action, query.From?.ToUniversalTime(), query.To?.ToUniversalTime()),
            new PageRequest(query.Page, query.Size),
            cancellationToken);
}
