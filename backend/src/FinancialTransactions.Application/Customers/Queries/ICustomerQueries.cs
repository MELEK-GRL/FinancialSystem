using FinancialTransactions.Application.Common.Paging;
using FinancialTransactions.Domain.Customers;

namespace FinancialTransactions.Application.Customers.Queries;

public interface ICustomerQueries
{
    Task<CustomerDto?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken);

    Task<PagedResult<CustomerSummaryDto>> ListAsync(string? search, CustomerStatus? status, PageRequest page, CancellationToken cancellationToken);
}
