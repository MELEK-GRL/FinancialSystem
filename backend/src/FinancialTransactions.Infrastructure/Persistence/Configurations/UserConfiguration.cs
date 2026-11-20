using FinancialTransactions.Domain.Customers;
using FinancialTransactions.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTransactions.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", table =>
        {
            table.HasCheckConstraint("ck_users_role", "role IN ('Admin', 'Customer')");
            table.HasCheckConstraint("ck_users_customer_role", "(role = 'Customer') = (customer_id IS NOT NULL)");
        });

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email).HasMaxLength(254).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(100).IsRequired();
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(10);
        builder.Property(user => user.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(user => user.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasIndex(user => user.CustomerId).IsUnique();
    }
}
