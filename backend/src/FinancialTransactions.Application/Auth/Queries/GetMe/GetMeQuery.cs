using MediatR;

namespace FinancialTransactions.Application.Auth.Queries.GetMe;

public sealed record GetMeQuery : IRequest<MeResult>;

public sealed record MeResult(
    Guid UserId,
    string Email,
    string Role,
    Guid? CustomerId,
    string? CustomerNo,
    string? FirstName,
    string? LastName);
