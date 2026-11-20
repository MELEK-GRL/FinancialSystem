using FinancialTransactions.Domain.Users;

namespace FinancialTransactions.Application.Users;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    void Add(User user);
}
