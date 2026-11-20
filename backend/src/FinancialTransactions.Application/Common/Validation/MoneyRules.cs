using FinancialTransactions.Application.Common.Localization;
using FluentValidation;

namespace FinancialTransactions.Application.Common.Validation;

public static class MoneyRules
{
    public const int DecimalPlaces = 2;

    public static IRuleBuilderOptions<T, decimal> MustBeValidAmount<T>(this IRuleBuilder<T, decimal> rule) =>
        rule
            .GreaterThan(0)
            .WithMessage(_ => Messages.Get(Messages.AmountMustBePositive))
            .Must(HasAtMostTwoDecimalPlaces)
            .WithMessage(_ => Messages.Get(Messages.AmountTooManyDecimals));

    private static bool HasAtMostTwoDecimalPlaces(decimal amount) => decimal.Round(amount, DecimalPlaces) == amount;
}
