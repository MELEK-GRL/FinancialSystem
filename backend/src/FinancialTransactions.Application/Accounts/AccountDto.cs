using FinancialTransactions.Domain.Accounts;

namespace FinancialTransactions.Application.Accounts;

public sealed record AccountDto(
    Guid Id,
    string AccountNo,
    Guid CustomerId,
    string Currency,
    decimal Balance,
    AccountStatus Status,
    DateTimeOffset CreatedAt)
{
    public static AccountDto From(Account account) =>
        new(account.Id, account.AccountNo, account.CustomerId, account.Currency, account.Balance, account.Status, account.CreatedAt);
}

public sealed record AccountBalanceDto(Guid AccountId, string AccountNo, decimal Balance, string Currency);
