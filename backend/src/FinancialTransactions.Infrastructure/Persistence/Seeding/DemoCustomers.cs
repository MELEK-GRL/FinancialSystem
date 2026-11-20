using FinancialTransactions.Application.Auth.Commands.Register;

namespace FinancialTransactions.Infrastructure.Persistence.Seeding;

public static class DemoCustomers
{
    public const string Password = "Demo12345";

    public static readonly IReadOnlyList<RegisterCommand> All =
    [
        new("Ayşe", "Yılmaz", "10000000146", "ayse.yilmaz@example.com", "5321112233", Password),
        new("Mehmet", "Demir", "12345678950", "mehmet.demir@example.com", "5332223344", Password),
        new("Zeynep", "Kaya", "98765432150", "zeynep.kaya@example.com", "5443334455", Password),
    ];
}
