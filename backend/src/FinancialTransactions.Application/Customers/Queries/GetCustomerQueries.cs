using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Domain.Customers;
using MediatR;

namespace FinancialTransactions.Application.Customers.Queries;

public sealed record GetMyProfileQuery : IRequest<CustomerDto>;

public sealed record GetCustomerQuery(Guid CustomerId) : IRequest<CustomerDto>;

public sealed class GetCustomerQueryHandler(ICurrentUser currentUser, ICustomerQueries customerQueries)
    : IRequestHandler<GetMyProfileQuery, CustomerDto>, IRequestHandler<GetCustomerQuery, CustomerDto>
{
    public Task<CustomerDto> Handle(GetMyProfileQuery query, CancellationToken cancellationToken) =>
        GetAsync(currentUser.RequireCustomerId(), cancellationToken);

    public Task<CustomerDto> Handle(GetCustomerQuery query, CancellationToken cancellationToken) =>
        GetAsync(query.CustomerId, cancellationToken);

    private async Task<CustomerDto> GetAsync(Guid customerId, CancellationToken cancellationToken) =>
        await customerQueries.GetByIdAsync(customerId, cancellationToken) ?? throw new CustomerNotFoundException();
}
