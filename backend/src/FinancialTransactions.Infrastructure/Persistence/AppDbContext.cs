using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Audit;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Transactions;
using FinancialTransactions.Domain.Users;
using FinancialTransactions.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>(CustomerConfiguration.CustomerNoSequence).StartsAt(100000001);
        modelBuilder.HasSequence<long>(AccountConfiguration.AccountNoSequence).StartsAt(1000000001);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
