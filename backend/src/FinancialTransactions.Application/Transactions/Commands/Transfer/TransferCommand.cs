using FinancialTransactions.Application.Accounts.Queries;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Application.Common.Validation;
using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Transactions;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Transactions.Commands.Transfer;

public sealed record TransferCommand(Guid SourceAccountId, string TargetAccountNo, decimal Amount, string? Description)
    : IRequest<TransactionDto>;

public sealed class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    public TransferCommandValidator()
    {
        RuleFor(command => command.SourceAccountId).NotEmpty();
        RuleFor(command => command.TargetAccountNo).MustBeValidAccountNo();
        RuleFor(command => command.Amount).MustBeValidAmount();
        RuleFor(command => command.Description).MaximumLength(TransactionRules.DescriptionMaxLength);
    }
}

public sealed class TransferCommandHandler(
    ICurrentUser currentUser,
    IAccountQueries accountQueries,
    IReferenceNumberGenerator referenceNumbers,
    TransactionExecutor executor) : IRequestHandler<TransferCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(TransferCommand command, CancellationToken cancellationToken)
    {
        var customerId = currentUser.RequireCustomerId();
        var source = await accountQueries.GetByIdAsync(command.SourceAccountId, cancellationToken);

        if (source is null || source.CustomerId != customerId)
        {
            throw new AccountNotFoundException();
        }

        var target = await accountQueries.GetByAccountNoAsync(AccountRules.NormalizeAccountNo(command.TargetAccountNo), cancellationToken)
            ?? throw new AccountNotFoundException();

        if (source.Id == target.Id)
        {
            throw new SameAccountTransferException();
        }

        var transaction = Transaction.CreateTransfer(
            await referenceNumbers.NextAsync(cancellationToken),
            source.Id,
            target.Id,
            command.Amount,
            command.Description,
            customerId,
            idempotencyKey: null);

        return await executor.ExecuteAsync(
            transaction,
            lockedAccounts =>
            {
                lockedAccounts[source.Id].Debit(transaction.Amount);
                lockedAccounts[target.Id].Credit(transaction.Amount);
            },
            cancellationToken);
    }
}
