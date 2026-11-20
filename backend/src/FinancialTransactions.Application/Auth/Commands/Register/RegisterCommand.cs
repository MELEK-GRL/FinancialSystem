using MediatR;

namespace FinancialTransactions.Application.Auth.Commands.Register;

public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string NationalId,
    string Email,
    string Phone,
    string Password) : IRequest<RegisterResult>;

public sealed record RegisterResult(Guid CustomerId, string CustomerNo);
