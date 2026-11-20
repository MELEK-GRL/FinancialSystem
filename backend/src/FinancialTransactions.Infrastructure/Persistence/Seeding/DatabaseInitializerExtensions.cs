using FinancialTransactions.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FinancialTransactions.Infrastructure.Persistence.Seeding;

public static class DatabaseInitializerExtensions
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        if (!services.GetRequiredService<IOptions<DatabaseOptions>>().Value.InitializeOnStartup)
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync(cancellationToken);
    }
}
