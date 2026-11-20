using FinancialTransactions.Application.Accounts.Queries;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Application.Common.Validation;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Transactions;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Transactions.Commands.Deposit;

public sealed record DepositCommand(Guid AccountId, decimal Amount, string? Description) : IRequest<TransactionDto>;

public sealed class DepositCommandValidator : AbstractValidator<DepositCommand>
{
    public DepositCommandValidator()
    {
        RuleFor(command => command.AccountId).NotEmpty();
        RuleFor(command => command.Amount).MustBeValidAmount();
        RuleFor(command => command.Description).MaximumLength(TransactionRules.DescriptionMaxLength);
    }
}

public sealed class DepositCommandHandler(
    ICurrentUser currentUser,
    IAccountQueries accountQueries,
    IReferenceNumberGenerator referenceNumbers,
    TransactionExecutor executor) : IRequestHandler<DepositCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(DepositCommand command, CancellationToken cancellationToken)
    {
        var customerId = currentUser.RequireCustomerId();
        var account = await accountQueries.GetByIdAsync(command.AccountId, cancellationToken);

        if (account is null || account.CustomerId != customerId)
        {
            throw new AccountNotFoundException();
        }

        var transaction = Transaction.CreateDeposit(
            await referenceNumbers.NextAsync(cancellationToken),
            account.Id,
            command.Amount,
            command.Description,
            customerId,
            idempotencyKey: null);

        return await executor.ExecuteAsync(
            transaction,
            lockedAccounts => lockedAccounts[account.Id].Credit(transaction.Amount),
            cancellationToken);
    }
}
