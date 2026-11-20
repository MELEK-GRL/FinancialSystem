using FinancialTransactions.Domain.Accounts;

namespace FinancialTransactions.Application.Accounts;

public interface IAccountRepository
{
    void Add(Account account);
}
