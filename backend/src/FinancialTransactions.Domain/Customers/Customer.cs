namespace FinancialTransactions.Domain.Customers;

public sealed class Customer
{
    private Customer()
    {
    }

    public Guid Id { get; private set; }

    public string CustomerNo { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string NationalId { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Phone { get; private set; } = string.Empty;

    public CustomerStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsActive => Status == CustomerStatus.Active;

    public static Customer Create(string firstName, string lastName, string nationalId, string email, string phone) =>
        new()
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            NationalId = nationalId.Trim(),
            Email = NormalizeEmail(email),
            Phone = phone.Trim(),
            Status = CustomerStatus.Active,
        };

    public void UpdateContact(string email, string phone)
    {
        Email = NormalizeEmail(email);
        Phone = phone.Trim();
    }

    public void Activate() => Status = CustomerStatus.Active;

    public void Deactivate() => Status = CustomerStatus.Passive;

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
