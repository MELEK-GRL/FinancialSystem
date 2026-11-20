using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Application.Common.Interfaces;
using MediatR;

namespace FinancialTransactions.Application.Auth.Queries.GetMe;

public sealed class GetMeQueryHandler(ICurrentUser currentUser, IUserQueries userQueries) : IRequestHandler<GetMeQuery, MeResult>
{
    public async Task<MeResult> Handle(GetMeQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        return await userQueries.GetMeAsync(userId, cancellationToken) ?? throw new InvalidCredentialsException();
    }
}
