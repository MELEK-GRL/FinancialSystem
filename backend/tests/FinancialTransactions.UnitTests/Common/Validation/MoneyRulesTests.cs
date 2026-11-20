using System.Globalization;
using FinancialTransactions.Application.Common.Validation;
using FluentValidation;

namespace FinancialTransactions.UnitTests.Common.Validation;

public class MoneyRulesTests
{
    private readonly AmountValidator validator = new();

    [Theory]
    [InlineData("0.01")]
    [InlineData("10")]
    [InlineData("10.5")]
    [InlineData("10.50")]
    [InlineData("999999999.99")]
    public void ValidAmounts_Pass(string amount)
    {
        Assert.True(validator.Validate(new AmountHolder(Parse(amount))).IsValid);
    }

    [Theory]
    [InlineData("0", "Tutar 0'dan büyük olmalı")]
    [InlineData("-5", "Tutar 0'dan büyük olmalı")]
    [InlineData("10.005", "Tutar en fazla 2 ondalık basamak içerebilir")]
    public void InvalidAmounts_FailWithTurkishMessage(string amount, string expectedMessage)
    {
        var errors = ValidateInCulture(Parse(amount), "tr");

        Assert.Equal([expectedMessage], errors);
    }

    [Fact]
    public void Messages_FollowCurrentLanguage()
    {
        var errors = ValidateInCulture(10.005m, "en");

        Assert.Equal(["Amount can have at most 2 decimal places"], errors);
    }

    private string[] ValidateInCulture(decimal amount, string culture)
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo(culture);

        try
        {
            return validator.Validate(new AmountHolder(amount)).Errors.Select(error => error.ErrorMessage).ToArray();
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    private static decimal Parse(string amount) => decimal.Parse(amount, CultureInfo.InvariantCulture);

    private sealed record AmountHolder(decimal Amount);

    private sealed class AmountValidator : AbstractValidator<AmountHolder>
    {
        public AmountValidator()
        {
            RuleFor(holder => holder.Amount).MustBeValidAmount();
        }
    }
}
