using FinancialTransactions.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTransactions.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public const string CustomerNoSequence = "customer_no_seq";

    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers", table =>
        {
            table.HasCheckConstraint("ck_customers_national_id", "national_id ~ '^[1-9][0-9]{10}$'");
            table.HasCheckConstraint("ck_customers_status", "status IN ('Active', 'Passive')");
        });

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.CustomerNo)
            .HasMaxLength(20)
            .HasDefaultValueSql($"'C' || nextval('{CustomerNoSequence}')")
            .ValueGeneratedOnAdd()
            .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

        builder.Property(customer => customer.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(customer => customer.LastName).HasMaxLength(50).IsRequired();
        builder.Property(customer => customer.NationalId).HasMaxLength(11).IsRequired();
        builder.Property(customer => customer.Email).HasMaxLength(254).IsRequired();
        builder.Property(customer => customer.Phone).HasMaxLength(15).IsRequired();
        builder.Property(customer => customer.Status).HasConversion<string>().HasMaxLength(10);

        builder.Property(customer => customer.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
        builder.Property(customer => customer.UpdatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAddOrUpdate();

        builder.Ignore(customer => customer.IsActive);

        builder.HasIndex(customer => customer.CustomerNo).IsUnique();
        builder.HasIndex(customer => customer.NationalId).IsUnique();
        builder.HasIndex(customer => customer.Email).IsUnique();
    }
}
