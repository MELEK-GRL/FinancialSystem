using FinancialTransactions.Application.Transactions.Commands.Transfer;

namespace FinancialTransactions.UnitTests.Transactions;

public class TransferCommandValidatorTests
{
    private readonly TransferCommandValidator validator = new();

    [Theory]
    [InlineData("TR1000000001")]
    [InlineData(" tr1000000001 ")]
    public void ValidCommand_Passes(string targetAccountNo)
    {
        Assert.True(validator.Validate(new TransferCommand(Guid.NewGuid(), targetAccountNo, 10m, "Kira")).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("TR123")]
    [InlineData("1000000001")]
    public void InvalidTargetAccountNo_Fails(string targetAccountNo)
    {
        var result = validator.Validate(new TransferCommand(Guid.NewGuid(), targetAccountNo, 10m, null));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(TransferCommand.TargetAccountNo));
    }

    [Fact]
    public void InvalidAmountOrSource_Fails()
    {
        Assert.False(validator.Validate(new TransferCommand(Guid.Empty, "TR1000000001", 10m, null)).IsValid);
        Assert.False(validator.Validate(new TransferCommand(Guid.NewGuid(), "TR1000000001", 0m, null)).IsValid);
    }
}
