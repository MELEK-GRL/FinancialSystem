namespace FinancialTransactions.Domain.Accounts;

public sealed class Account
{
    public const string DefaultCurrency = "TRY";

    private Account()
    {
    }

    public Guid Id { get; private set; }

    public string AccountNo { get; private set; } = string.Empty;

    public Guid CustomerId { get; private set; }

    public string Currency { get; private set; } = DefaultCurrency;

    public decimal Balance { get; private set; }

    public AccountStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsActive => Status == AccountStatus.Active;

    public static Account Open(Guid customerId) =>
        new()
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Currency = DefaultCurrency,
            Balance = 0m,
            Status = AccountStatus.Active,
        };

    public void EnsureActive()
    {
        if (!IsActive)
        {
            throw new AccountInactiveException();
        }
    }

    public void Credit(decimal amount)
    {
        EnsurePositive(amount);
        Balance += amount;
    }

    public void Debit(decimal amount)
    {
        EnsurePositive(amount);

        if (Balance < amount)
        {
            throw new InsufficientBalanceException();
        }

        Balance -= amount;
    }

    public void Activate() => Status = AccountStatus.Active;

    public void Deactivate() => Status = AccountStatus.Passive;

    private static void EnsurePositive(decimal amount) =>
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
}
