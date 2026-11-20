using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Application.Users;
using FinancialTransactions.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinancialTransactions.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokenService,
    ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByEmailAsync(command.Email.Trim().ToLowerInvariant(), cancellationToken);

        var passwordMatches = passwordHasher.Verify(command.Password, user?.PasswordHash);

        if (user is null || !passwordMatches)
        {
            logger.LogWarning("Login failed");
            throw new InvalidCredentialsException();
        }

        var token = tokenService.Create(user);
        var role = user.Role == UserRole.Admin ? AppRoles.Admin : AppRoles.Customer;

        return new LoginResult(token.Token, token.ExpiresAt, role);
    }
}
