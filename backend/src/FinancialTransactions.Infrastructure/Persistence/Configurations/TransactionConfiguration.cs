using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTransactions.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions", table =>
        {
            table.HasCheckConstraint("ck_transactions_amount_positive", "amount > 0");
            table.HasCheckConstraint("ck_transactions_type", "type IN ('Deposit', 'Withdrawal', 'Transfer')");
            table.HasCheckConstraint("ck_transactions_status", "status IN ('Pending', 'Success', 'Failed')");
            table.HasCheckConstraint("ck_transactions_currency", "currency ~ '^[A-Z]{3}$'");
            table.HasCheckConstraint(
                "ck_transactions_accounts_match_type",
                """
                (type = 'Deposit' AND source_account_id IS NULL AND target_account_id IS NOT NULL)
                OR (type = 'Withdrawal' AND source_account_id IS NOT NULL AND target_account_id IS NULL)
                OR (type = 'Transfer' AND source_account_id IS NOT NULL AND target_account_id IS NOT NULL
                    AND source_account_id <> target_account_id)
                """);
        });

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.ReferenceNo).HasMaxLength(30).IsRequired();
        builder.Property(transaction => transaction.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(transaction => transaction.Status).HasConversion<string>().HasMaxLength(10);
        builder.Property(transaction => transaction.Amount).HasPrecision(19, 4);
        builder.Property(transaction => transaction.Currency).HasMaxLength(3).IsFixedLength();
        builder.Property(transaction => transaction.Description).HasMaxLength(250);
        builder.Property(transaction => transaction.FailureReason).HasMaxLength(50);
        builder.Property(transaction => transaction.IdempotencyKey).HasMaxLength(100);
        builder.Property(transaction => transaction.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder.Property(transaction => transaction.CompletedAt)
            .ValueGeneratedOnUpdate()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasOne<Account>().WithMany().HasForeignKey(transaction => transaction.SourceAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Account>().WithMany().HasForeignKey(transaction => transaction.TargetAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Customer>().WithMany().HasForeignKey(transaction => transaction.InitiatedByCustomerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(transaction => transaction.ReferenceNo).IsUnique();
        builder.HasIndex(transaction => new { transaction.InitiatedByCustomerId, transaction.IdempotencyKey })
            .IsUnique()
            .HasFilter("idempotency_key IS NOT NULL");
        builder.HasIndex(transaction => new { transaction.InitiatedByCustomerId, transaction.CreatedAt });
        builder.HasIndex(transaction => new { transaction.SourceAccountId, transaction.CreatedAt }).IsDescending(false, true);
        builder.HasIndex(transaction => new { transaction.TargetAccountId, transaction.CreatedAt }).IsDescending(false, true);
        builder.HasIndex(transaction => new { transaction.Status, transaction.CreatedAt });
        builder.HasIndex(transaction => transaction.CreatedAt);
        builder.HasIndex(transaction => transaction.CreatedAt, "ix_transactions_pending_created_at")
            .HasDatabaseName("ix_transactions_pending_created_at")
            .HasFilter("status = 'Pending'");
    }
}
