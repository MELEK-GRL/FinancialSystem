using System.Text.RegularExpressions;
using FinancialTransactions.Application.Common.Localization;
using FluentValidation;

namespace FinancialTransactions.Application.Common.Validation;

public static partial class AccountRules
{
    public static IRuleBuilderOptions<T, string> MustBeValidAccountNo<T>(this IRuleBuilder<T, string> rule) =>
        rule
            .NotEmpty()
            .Must(accountNo => AccountNoPattern().IsMatch(NormalizeAccountNo(accountNo)))
            .WithMessage(_ => Messages.Get(Messages.AccountNoInvalid));

    public static string NormalizeAccountNo(string? accountNo) => accountNo?.Trim().ToUpperInvariant() ?? string.Empty;

    [GeneratedRegex("^TR[0-9]{10}$")]
    private static partial Regex AccountNoPattern();
}
