using FinancialTransactions.Application.Accounts;
using FinancialTransactions.Application.Accounts.Queries;
using FinancialTransactions.Application.Auth.Queries.GetMe;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Customers;
using FinancialTransactions.Application.Customers.Queries;
using FinancialTransactions.Application.Users;
using FinancialTransactions.Infrastructure.Options;
using FinancialTransactions.Infrastructure.Persistence;
using FinancialTransactions.Infrastructure.Persistence.Seeding;
using FinancialTransactions.Infrastructure.Queries;
using FinancialTransactions.Infrastructure.Repositories;
using FinancialTransactions.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        services.AddOptions<SeedOptions>().BindConfiguration(SeedOptions.SectionName);

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
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserQueries, UserQueries>();
        services.AddScoped<IAccountQueries, AccountQueries>();
        services.AddScoped<ICustomerQueries, CustomerQueries>();
        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}
