using FinancialTransactions.Application.Common.Paging;
using FinancialTransactions.Application.Customers;
using FinancialTransactions.Application.Customers.Queries;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Queries;

public sealed class CustomerQueries(AppDbContext dbContext) : ICustomerQueries
{
    public Task<CustomerDto?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken) =>
        dbContext.Customers.AsNoTracking()
            .Where(customer => customer.Id == customerId)
            .Select(customer => new CustomerDto(
                customer.Id,
                customer.CustomerNo,
                customer.FirstName,
                customer.LastName,
                customer.NationalId,
                customer.Email,
                customer.Phone,
                customer.Status,
                customer.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<PagedResult<CustomerSummaryDto>> ListAsync(
        string? search,
        CustomerStatus? status,
        PageRequest page,
        CancellationToken cancellationToken)
    {
        var customers = dbContext.Customers.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            var pattern = $"%{EscapeLikePattern(search)}%";
            customers = customers.Where(customer =>
                EF.Functions.ILike(customer.FirstName + " " + customer.LastName, pattern)
                || EF.Functions.ILike(customer.Email, pattern)
                || EF.Functions.ILike(customer.CustomerNo, pattern)
                || EF.Functions.ILike(customer.NationalId, pattern));
        }

        if (status is not null)
        {
            customers = customers.Where(customer => customer.Status == status);
        }

        return customers
            .OrderByDescending(customer => customer.CreatedAt)
            .ThenBy(customer => customer.Id)
            .Select(customer => new CustomerSummaryDto(
                customer.Id,
                customer.CustomerNo,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.Status,
                customer.CreatedAt))
            .ToPagedResultAsync(page, cancellationToken);
    }

    private static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
