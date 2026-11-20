using FinancialTransactions.Domain.Users;

namespace FinancialTransactions.Application.Common.Interfaces;

public interface IJwtTokenService
{
    AccessToken Create(User user);
}

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);
