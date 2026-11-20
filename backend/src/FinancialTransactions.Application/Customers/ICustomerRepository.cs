using FinancialTransactions.Domain.Customers;

namespace FinancialTransactions.Application.Customers;

public interface ICustomerRepository
{
    Task<bool> ExistsAsync(string nationalId, string email, CancellationToken cancellationToken);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(Customer customer);
}
