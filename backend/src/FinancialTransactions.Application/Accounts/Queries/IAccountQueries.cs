namespace FinancialTransactions.Application.Accounts.Queries;

public interface IAccountQueries
{
    Task<IReadOnlyList<AccountDto>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken);

    Task<AccountDto?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken);

    Task<AccountOwnerRow?> GetOwnerByAccountNoAsync(string accountNo, CancellationToken cancellationToken);
}
