using FinancialTransactions.Application.Common.Paging;
using FinancialTransactions.Domain.Customers;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Customers.Queries;

public sealed record ListCustomersQuery(string? Search, CustomerStatus? Status, int Page, int Size)
    : IRequest<PagedResult<CustomerSummaryDto>>;

public sealed class ListCustomersQueryValidator : AbstractValidator<ListCustomersQuery>
{
    public const int SearchMaxLength = 100;

    public ListCustomersQueryValidator()
    {
        RuleFor(query => query.Search).MaximumLength(SearchMaxLength);
        RuleFor(query => query.Status).IsInEnum();
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.Size).InclusiveBetween(1, PageRequest.MaxSize);
    }
}

public sealed class ListCustomersQueryHandler(ICustomerQueries customerQueries)
    : IRequestHandler<ListCustomersQuery, PagedResult<CustomerSummaryDto>>
{
    public Task<PagedResult<CustomerSummaryDto>> Handle(ListCustomersQuery query, CancellationToken cancellationToken) =>
        customerQueries.ListAsync(query.Search?.Trim(), query.Status, new PageRequest(query.Page, query.Size), cancellationToken);
}
