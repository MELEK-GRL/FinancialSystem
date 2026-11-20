using FinancialTransactions.Application.Transactions.Commands.Deposit;

namespace FinancialTransactions.UnitTests.Transactions;

public class DepositCommandValidatorTests
{
    private readonly DepositCommandValidator validator = new();

    [Fact]
    public void ValidCommand_Passes()
    {
        Assert.True(validator.Validate(new DepositCommand(Guid.NewGuid(), 100.50m, "Maaş")).IsValid);
    }

    [Fact]
    public void EmptyAccountId_Fails()
    {
        Assert.False(validator.Validate(new DepositCommand(Guid.Empty, 10m, null)).IsValid);
    }

    [Fact]
    public void TooLongDescription_Fails()
    {
        Assert.False(validator.Validate(new DepositCommand(Guid.NewGuid(), 10m, new string('a', 251))).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveAmount_Fails(int amount)
    {
        Assert.False(validator.Validate(new DepositCommand(Guid.NewGuid(), amount, null)).IsValid);
    }
}
