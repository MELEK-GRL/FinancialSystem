namespace FinancialTransactions.Infrastructure.Options;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminEmail { get; init; } = string.Empty;

    public string AdminPassword { get; init; } = string.Empty;

    public bool CreateDemoCustomers { get; init; }
}
