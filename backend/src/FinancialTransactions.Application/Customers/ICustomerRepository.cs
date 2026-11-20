using FinancialTransactions.Domain.Customers;

namespace FinancialTransactions.Application.Customers;

public interface ICustomerRepository
{
    Task<bool> ExistsAsync(string nationalId, string email, CancellationToken cancellationToken);

    Task<bool> EmailTakenByOtherAsync(string email, Guid customerId, CancellationToken cancellationToken);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> AllActiveAsync(IReadOnlyCollection<Guid> customerIds, CancellationToken cancellationToken);

    void Add(Customer customer);
}
