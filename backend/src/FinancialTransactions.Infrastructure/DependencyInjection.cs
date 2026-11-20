using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Infrastructure.Options;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FinancialTransactions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var database = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

            options
                .UseNpgsql(database.ConnectionString, npgsql => npgsql
                    .CommandTimeout(database.CommandTimeoutSeconds)
                    .MigrationsHistoryTable("__ef_migrations_history"))
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
