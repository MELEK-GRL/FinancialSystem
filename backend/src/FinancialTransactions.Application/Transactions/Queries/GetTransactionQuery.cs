using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Domain.Transactions;
using MediatR;

namespace FinancialTransactions.Application.Transactions.Queries;

public sealed record GetTransactionQuery(string ReferenceNo) : IRequest<TransactionDto>;

public sealed class GetTransactionQueryHandler(ICurrentUser currentUser, ITransactionQueries transactionQueries)
    : IRequestHandler<GetTransactionQuery, TransactionDto>
{
    public async Task<TransactionDto> Handle(GetTransactionQuery query, CancellationToken cancellationToken)
    {
        var referenceNo = query.ReferenceNo.Trim().ToUpperInvariant();
        var found = await transactionQueries.GetByReferenceNoAsync(referenceNo, cancellationToken);

        if (found is null || !CanSee(found))
        {
            throw new TransactionNotFoundException();
        }

        return found.Transaction;
    }

    private bool CanSee(TransactionWithOwners found)
    {
        if (currentUser.IsAdmin)
        {
            return true;
        }

        var customerId = currentUser.CustomerId;

        return customerId == found.InitiatedByCustomerId
            || customerId == found.SourceCustomerId
            || (customerId == found.TargetCustomerId && found.Transaction.Status == TransactionStatus.Success);
    }
}
