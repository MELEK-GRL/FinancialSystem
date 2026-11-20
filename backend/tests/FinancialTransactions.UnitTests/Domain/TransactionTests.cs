using FinancialTransactions.Domain.Transactions;

namespace FinancialTransactions.UnitTests.Domain;

public class TransactionTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();

    [Fact]
    public void CreateDeposit_HasOnlyTargetAccount_AndStartsPending()
    {
        var target = Guid.NewGuid();

        var transaction = Transaction.CreateDeposit("REF1", target, 100m, "  Maaş  ", CustomerId, "key-1");

        Assert.Equal(TransactionType.Deposit, transaction.Type);
        Assert.Equal(TransactionStatus.Pending, transaction.Status);
        Assert.Null(transaction.SourceAccountId);
        Assert.Equal(target, transaction.TargetAccountId);
        Assert.Equal("Maaş", transaction.Description);
        Assert.Equal("TRY", transaction.Currency);
        Assert.Equal("key-1", transaction.IdempotencyKey);
    }

    [Fact]
    public void CreateWithdrawal_HasOnlySourceAccount()
    {
        var source = Guid.NewGuid();

        var transaction = Transaction.CreateWithdrawal("REF2", source, 50m, null, CustomerId, null);

        Assert.Equal(TransactionType.Withdrawal, transaction.Type);
        Assert.Equal(source, transaction.SourceAccountId);
        Assert.Null(transaction.TargetAccountId);
    }

    [Fact]
    public void CreateTransfer_HasBothAccounts_AndBlankDescriptionBecomesNull()
    {
        var transaction = Transaction.CreateTransfer("REF3", Guid.NewGuid(), Guid.NewGuid(), 25m, "   ", CustomerId, null);

        Assert.Equal(TransactionType.Transfer, transaction.Type);
        Assert.NotNull(transaction.SourceAccountId);
        Assert.NotNull(transaction.TargetAccountId);
        Assert.Null(transaction.Description);
    }

    [Fact]
    public void MarkSucceeded_FromPending_SetsSuccess()
    {
        var transaction = Transaction.CreateDeposit("REF4", Guid.NewGuid(), 10m, null, CustomerId, null);

        transaction.MarkSucceeded();

        Assert.Equal(TransactionStatus.Success, transaction.Status);
        Assert.Null(transaction.FailureReason);
    }

    [Fact]
    public void MarkFailed_FromPending_SetsFailedWithReason()
    {
        var transaction = Transaction.CreateWithdrawal("REF5", Guid.NewGuid(), 10m, null, CustomerId, null);

        transaction.MarkFailed("INSUFFICIENT_BALANCE");

        Assert.Equal(TransactionStatus.Failed, transaction.Status);
        Assert.Equal("INSUFFICIENT_BALANCE", transaction.FailureReason);
    }

    [Fact]
    public void CompletedTransaction_CannotChangeStatusAgain()
    {
        var transaction = Transaction.CreateDeposit("REF6", Guid.NewGuid(), 10m, null, CustomerId, null);
        transaction.MarkSucceeded();

        Assert.Throws<InvalidOperationException>(transaction.MarkSucceeded);
        Assert.Throws<InvalidOperationException>(() => transaction.MarkFailed("X"));
        Assert.Equal(TransactionStatus.Success, transaction.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void NonPositiveAmount_IsRejected(int amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Transaction.CreateDeposit("REF7", Guid.NewGuid(), amount, null, CustomerId, null));
    }
}
