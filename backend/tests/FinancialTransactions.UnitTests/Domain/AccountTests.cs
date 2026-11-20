using FinancialTransactions.Domain.Accounts;

namespace FinancialTransactions.UnitTests.Domain;

public class AccountTests
{
    [Fact]
    public void Open_StartsActiveWithZeroBalanceInTry()
    {
        var customerId = Guid.NewGuid();

        var account = Account.Open(customerId);

        Assert.Equal(customerId, account.CustomerId);
        Assert.Equal(0m, account.Balance);
        Assert.Equal("TRY", account.Currency);
        Assert.True(account.IsActive);
    }

    [Fact]
    public void Credit_IncreasesBalance()
    {
        var account = Account.Open(Guid.NewGuid());

        account.Credit(100.25m);
        account.Credit(0.75m);

        Assert.Equal(101m, account.Balance);
    }

    [Fact]
    public void Debit_DecreasesBalance()
    {
        var account = Account.Open(Guid.NewGuid());
        account.Credit(100m);

        account.Debit(40.50m);

        Assert.Equal(59.50m, account.Balance);
    }

    [Fact]
    public void Debit_ExactBalance_LeavesZero()
    {
        var account = Account.Open(Guid.NewGuid());
        account.Credit(50m);

        account.Debit(50m);

        Assert.Equal(0m, account.Balance);
    }

    [Fact]
    public void Debit_MoreThanBalance_ThrowsAndKeepsBalance()
    {
        var account = Account.Open(Guid.NewGuid());
        account.Credit(10m);

        Assert.Throws<InsufficientBalanceException>(() => account.Debit(10.01m));
        Assert.Equal(10m, account.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreditOrDebit_NonPositiveAmount_Throws(int amount)
    {
        var account = Account.Open(Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(() => account.Credit(amount));
        Assert.Throws<ArgumentOutOfRangeException>(() => account.Debit(amount));
    }

    [Fact]
    public void EnsureActive_ThrowsForPassiveAccount()
    {
        var account = Account.Open(Guid.NewGuid());

        account.Deactivate();

        Assert.Throws<AccountInactiveException>(account.EnsureActive);

        account.Activate();
        account.EnsureActive();
    }
}
