using System.ComponentModel.DataAnnotations;

namespace FinancialTransactions.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;

    [Range(1, 300)]
    public int CommandTimeoutSeconds { get; init; } = 30;

    public bool InitializeOnStartup { get; init; }
}
