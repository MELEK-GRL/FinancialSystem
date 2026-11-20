using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Users;
using FinancialTransactions.Domain.Users;
using FinancialTransactions.Infrastructure.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinancialTransactions.Infrastructure.Persistence.Seeding;

public sealed class DatabaseInitializer(
    AppDbContext dbContext,
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ISender sender,
    IOptions<SeedOptions> seedOptions,
    ILogger<DatabaseInitializer> logger)
{
    private readonly SeedOptions seed = seedOptions.Value;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Database migrations applied");

        await SeedAdminAsync(cancellationToken);

        if (seed.CreateDemoCustomers)
        {
            await SeedDemoCustomersAsync(cancellationToken);
        }
    }

    private async Task SeedAdminAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(seed.AdminEmail) || string.IsNullOrWhiteSpace(seed.AdminPassword))
        {
            logger.LogWarning("Admin seed skipped because admin credentials are not configured");
            return;
        }

        var email = seed.AdminEmail.Trim().ToLowerInvariant();

        if (await users.EmailExistsAsync(email, cancellationToken))
        {
            return;
        }

        var admin = User.CreateAdmin(email, passwordHasher.Hash(seed.AdminPassword));
        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            users.Add(admin);
            return Task.CompletedTask;
        }, cancellationToken);

        logger.LogInformation("Admin user created");
    }

    private async Task SeedDemoCustomersAsync(CancellationToken cancellationToken)
    {
        foreach (var customer in DemoCustomers.All)
        {
            var exists = await dbContext.Customers.AnyAsync(c => c.NationalId == customer.NationalId, cancellationToken);

            if (!exists)
            {
                await sender.Send(customer, cancellationToken);
                logger.LogInformation("Demo customer created");
            }
        }
    }
}
