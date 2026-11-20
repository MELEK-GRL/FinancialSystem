using System.Text.RegularExpressions;
using FinancialTransactions.Application.Common.Localization;
using FluentValidation;

namespace FinancialTransactions.Application.Common.Validation;

public static partial class TransactionRules
{
    public const int DescriptionMaxLength = 250;

    public static IRuleBuilderOptions<T, string?> MustBeValidIdempotencyKey<T>(this IRuleBuilder<T, string?> rule) =>
        rule
            .Must(key => key is null || IdempotencyKeyPattern().IsMatch(key))
            .WithMessage(_ => Messages.Get(Messages.IdempotencyKeyInvalid));

    [GeneratedRegex("^[A-Za-z0-9_-]{8,100}$")]
    private static partial Regex IdempotencyKeyPattern();
}
