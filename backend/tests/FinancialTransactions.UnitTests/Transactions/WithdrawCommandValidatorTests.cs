using FinancialTransactions.Application.Transactions.Commands.Withdraw;

namespace FinancialTransactions.UnitTests.Transactions;

public class WithdrawCommandValidatorTests
{
    private readonly WithdrawCommandValidator validator = new();

    [Fact]
    public void ValidCommand_Passes()
    {
        Assert.True(validator.Validate(new WithdrawCommand(Guid.NewGuid(), 50m, "ATM")).IsValid);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("10.001")]
    public void InvalidAmount_Fails(string amount)
    {
        var command = new WithdrawCommand(Guid.NewGuid(), decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture), null);

        Assert.False(validator.Validate(command).IsValid);
    }

    [Fact]
    public void EmptyAccountId_Fails()
    {
        Assert.False(validator.Validate(new WithdrawCommand(Guid.Empty, 10m, null)).IsValid);
    }
}
