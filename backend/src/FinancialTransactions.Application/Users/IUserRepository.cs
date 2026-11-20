using FinancialTransactions.Domain.Users;

namespace FinancialTransactions.Application.Users;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);

    void Add(User user);
}
