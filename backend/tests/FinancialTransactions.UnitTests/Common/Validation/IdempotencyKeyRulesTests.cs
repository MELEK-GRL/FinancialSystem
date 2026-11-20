using FinancialTransactions.Application.Transactions.Commands.Deposit;

namespace FinancialTransactions.UnitTests.Common.Validation;

public class IdempotencyKeyRulesTests
{
    private readonly DepositCommandValidator validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("3f2b9c1e-8d4a-4b7f-9e2c-1a5d6f7b8c9d")]
    [InlineData("order_12345")]
    public void ValidOrMissingKey_Passes(string? key)
    {
        Assert.True(validator.Validate(new DepositCommand(Guid.NewGuid(), 10m, null, key)).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("has spaces in key")]
    [InlineData("çok-uzun-olmayan-anahtar")]
    public void InvalidKey_Fails(string key)
    {
        var result = validator.Validate(new DepositCommand(Guid.NewGuid(), 10m, null, key));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(DepositCommand.IdempotencyKey));
    }

    [Fact]
    public void TooLongKey_Fails()
    {
        Assert.False(validator.Validate(new DepositCommand(Guid.NewGuid(), 10m, null, new string('a', 101))).IsValid);
    }
}
