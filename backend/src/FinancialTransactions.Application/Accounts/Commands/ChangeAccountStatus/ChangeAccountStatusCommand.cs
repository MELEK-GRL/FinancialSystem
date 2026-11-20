using FinancialTransactions.Domain.Accounts;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Accounts.Commands.ChangeAccountStatus;

public sealed record ChangeAccountStatusCommand(Guid AccountId, AccountStatus Status, string Reason) : IRequest<AccountDto>;

public sealed class ChangeAccountStatusCommandValidator : AbstractValidator<ChangeAccountStatusCommand>
{
    public const int ReasonMinLength = 5;
    public const int ReasonMaxLength = 250;

    public ChangeAccountStatusCommandValidator()
    {
        RuleFor(command => command.Status).IsInEnum();
        RuleFor(command => command.Reason).NotEmpty().Length(ReasonMinLength, ReasonMaxLength);
    }
}
