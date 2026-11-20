using System.ComponentModel.DataAnnotations;
using System.Globalization;
using FinancialTransactions.Application.Common.Options;

namespace FinancialTransactions.UnitTests.Common.Options;

public class TransactionOptionsTests
{
    [Theory]
    [InlineData("0.01")]
    [InlineData("100000")]
    public void MaxAmount_WithinRange_IsValid(string maxAmount)
    {
        var options = new TransactionOptions { MaxAmount = decimal.Parse(maxAmount, CultureInfo.InvariantCulture) };

        Assert.True(IsValid(options));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void MaxAmount_ZeroOrNegative_IsInvalid(string maxAmount)
    {
        var options = new TransactionOptions { MaxAmount = decimal.Parse(maxAmount, CultureInfo.InvariantCulture) };

        Assert.False(IsValid(options));
    }

    private static bool IsValid(object options) =>
        Validator.TryValidateObject(options, new ValidationContext(options), null, validateAllProperties: true);
}
