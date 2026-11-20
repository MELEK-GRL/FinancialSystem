using FinancialTransactions.Application.Common.Validation;
using FluentValidation;

namespace FinancialTransactions.Application.Auth.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.FirstName).MustBeValidName();
        RuleFor(command => command.LastName).MustBeValidName();
        RuleFor(command => command.NationalId).MustBeValidNationalId();
        RuleFor(command => command.Email).MustBeValidEmail();
        RuleFor(command => command.Phone).MustBeValidPhone();
        RuleFor(command => command.Password).MustBeStrongPassword();
    }
}
