using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTransactions.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public const string AccountNoSequence = "account_no_seq";

    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts", table =>
        {
            table.HasCheckConstraint("ck_accounts_balance_not_negative", "balance >= 0");
            table.HasCheckConstraint("ck_accounts_currency", "currency ~ '^[A-Z]{3}$'");
            table.HasCheckConstraint("ck_accounts_status", "status IN ('Active', 'Passive')");
        });

        builder.HasKey(account => account.Id);

        builder.Property(account => account.AccountNo)
            .HasMaxLength(20)
            .HasDefaultValueSql($"'TR' || nextval('{AccountNoSequence}')")
            .ValueGeneratedOnAdd()
            .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

        builder.Property(account => account.Currency).HasMaxLength(3).IsFixedLength().HasDefaultValue(Account.DefaultCurrency);
        builder.Property(account => account.Balance).HasPrecision(19, 4);
        builder.Property(account => account.Status).HasConversion<string>().HasMaxLength(10);

        builder.Property(account => account.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
        builder.Property(account => account.UpdatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAddOrUpdate();

        builder.Ignore(account => account.IsActive);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(account => account.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(account => account.AccountNo).IsUnique();
        builder.HasIndex(account => account.CustomerId);
    }
}
