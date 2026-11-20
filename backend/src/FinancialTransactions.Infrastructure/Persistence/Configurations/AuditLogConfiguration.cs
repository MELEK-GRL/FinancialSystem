using FinancialTransactions.Domain.Accounts;
using FinancialTransactions.Domain.Audit;
using FinancialTransactions.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTransactions.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs", table =>
        {
            table.HasCheckConstraint("ck_audit_logs_action", "action IN ('AccountActivated', 'AccountDeactivated')");
            table.HasCheckConstraint("ck_audit_logs_status_changed", "old_status <> new_status");
        });

        builder.HasKey(auditLog => auditLog.Id);

        builder.Property(auditLog => auditLog.Action).HasConversion<string>().HasMaxLength(30);
        builder.Property(auditLog => auditLog.OldStatus).HasConversion<string>().HasMaxLength(10);
        builder.Property(auditLog => auditLog.NewStatus).HasConversion<string>().HasMaxLength(10);
        builder.Property(auditLog => auditLog.Reason).HasMaxLength(250).IsRequired();
        builder.Property(auditLog => auditLog.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(auditLog => auditLog.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(auditLog => auditLog.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(auditLog => auditLog.PerformedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(auditLog => new { auditLog.AccountId, auditLog.CreatedAt }).IsDescending(false, true);
    }
}
