using FinancialTransactions.Application.Common.Interfaces;

namespace FinancialTransactions.Infrastructure.Security;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword("timing-protection", WorkFactor);

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string? passwordHash)
    {
        if (passwordHash is null)
        {
            BCrypt.Net.BCrypt.Verify(password, DummyHash);
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
