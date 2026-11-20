using System.Text.Json.Serialization;
using FinancialTransactions.Application.Accounts.Queries;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Application.Common.Validation;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Transactions;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Transactions.Commands.Withdraw;

public sealed record WithdrawCommand(Guid AccountId, decimal Amount, string? Description, [property: JsonIgnore] string? IdempotencyKey = null)
    : IRequest<TransactionDto>;

public sealed class WithdrawCommandValidator : AbstractValidator<WithdrawCommand>
{
    public WithdrawCommandValidator()
    {
        RuleFor(command => command.AccountId).NotEmpty();
        RuleFor(command => command.Amount).MustBeValidAmount();
        RuleFor(command => command.Description).MaximumLength(TransactionRules.DescriptionMaxLength);
        RuleFor(command => command.IdempotencyKey).MustBeValidIdempotencyKey();
    }
}

public sealed class WithdrawCommandHandler(
    ICurrentUser currentUser,
    IAccountQueries accountQueries,
    IReferenceNumberGenerator referenceNumbers,
    TransactionExecutor executor) : IRequestHandler<WithdrawCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(WithdrawCommand command, CancellationToken cancellationToken)
    {
        var customerId = currentUser.RequireCustomerId();
        var account = await accountQueries.GetByIdAsync(command.AccountId, cancellationToken);

        if (account is null || account.CustomerId != customerId)
        {
            throw new AccountNotFoundException();
        }

        var transaction = Transaction.CreateWithdrawal(
            await referenceNumbers.NextAsync(cancellationToken),
            account.Id,
            command.Amount,
            command.Description,
            customerId,
            command.IdempotencyKey);

        return await executor.ExecuteAsync(
            transaction,
            lockedAccounts => lockedAccounts[account.Id].Debit(transaction.Amount),
            cancellationToken);
    }
}
