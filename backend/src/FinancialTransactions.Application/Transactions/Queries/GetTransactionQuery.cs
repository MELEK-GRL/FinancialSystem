using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
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

        if (found is null || !found.InvolvedCustomerIds.Any(currentUser.CanAccessCustomer))
        {
            throw new TransactionNotFoundException();
        }

        return found.Transaction;
    }
}
