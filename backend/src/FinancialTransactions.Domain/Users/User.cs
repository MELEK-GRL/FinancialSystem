namespace FinancialTransactions.Domain.Users;

public sealed class User
{
    private User()
    {
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public Guid? CustomerId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static User CreateCustomerUser(string email, string passwordHash, Guid customerId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = UserRole.Customer,
            CustomerId = customerId,
        };

    public static User CreateAdmin(string email, string passwordHash) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = UserRole.Admin,
        };

    public void ChangeEmail(string email) => Email = email.Trim().ToLowerInvariant();
}
