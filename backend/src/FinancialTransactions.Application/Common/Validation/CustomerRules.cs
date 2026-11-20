using FinancialTransactions.Application.Common.Localization;
using FinancialTransactions.Domain.Customers;
using FluentValidation;

namespace FinancialTransactions.Application.Common.Validation;

public static class CustomerRules
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 50;
    public const int EmailMaxLength = 254;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 72;

    public static IRuleBuilderOptions<T, string> MustBeValidName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().Length(NameMinLength, NameMaxLength);

    public static IRuleBuilderOptions<T, string> MustBeValidEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(EmailMaxLength).EmailAddress();

    public static IRuleBuilderOptions<T, string> MustBeValidNationalId<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().Must(NationalId.IsValid).WithMessage(_ => Messages.Get(Messages.NationalIdInvalid));

    public static IRuleBuilderOptions<T, string> MustBeValidPhone<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().Matches("^5[0-9]{9}$").WithMessage(_ => Messages.Get(Messages.PhoneInvalid));

    public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule
            .NotEmpty()
            .Length(PasswordMinLength, PasswordMaxLength)
            .Matches("[A-Za-z]").WithMessage(_ => Messages.Get(Messages.PasswordWeak))
            .Matches("[0-9]").WithMessage(_ => Messages.Get(Messages.PasswordWeak));
}
