using MediatR;

namespace FinancialTransactions.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public sealed record LoginResult(string AccessToken, DateTimeOffset ExpiresAt, string Role);
