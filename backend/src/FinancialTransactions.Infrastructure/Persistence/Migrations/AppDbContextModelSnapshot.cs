using System;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinancialTransactions.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.23")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.HasSequence("account_no_seq")
                .StartsAt(1000000001L);

            modelBuilder.HasSequence("customer_no_seq")
                .StartsAt(100000001L);

            modelBuilder.Entity("FinancialTransactions.Domain.Accounts.Account", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("AccountNo")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("account_no")
                        .HasDefaultValueSql("'TR' || nextval('account_no_seq')");

                    b.Property<decimal>("Balance")
                        .HasPrecision(19, 4)
                        .HasColumnType("numeric(19,4)")
                        .HasColumnName("balance");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(3)
                        .HasColumnType("character(3)")
                        .HasDefaultValue("TRY")
                        .HasColumnName("currency")
                        .IsFixedLength();

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid")
                        .HasColumnName("customer_id");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("status");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Id")
                        .HasName("pk_accounts");

                    b.HasIndex("AccountNo")
                        .IsUnique()
                        .HasDatabaseName("ix_accounts_account_no");

                    b.HasIndex("CustomerId")
                        .HasDatabaseName("ix_accounts_customer_id");

                    b.ToTable("accounts", null, t =>
                        {
                            t.HasCheckConstraint("ck_accounts_balance_not_negative", "balance >= 0");

                            t.HasCheckConstraint("ck_accounts_currency", "currency ~ '^[A-Z]{3}$'");

                            t.HasCheckConstraint("ck_accounts_status", "status IN ('Active', 'Passive')");
                        });
                });

            modelBuilder.Entity("FinancialTransactions.Domain.Customers.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("CustomerNo")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("customer_no")
                        .HasDefaultValueSql("'C' || nextval('customer_no_seq')");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(254)
                        .HasColumnType("character varying(254)")
                        .HasColumnName("email");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("first_name");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("last_name");

                    b.Property<string>("NationalId")
                        .IsRequired()
                        .HasMaxLength(11)
                        .HasColumnType("character varying(11)")
                        .HasColumnName("national_id");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("character varying(15)")
                        .HasColumnName("phone");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("status");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Id")
                        .HasName("pk_customers");

                    b.HasIndex("CustomerNo")
                        .IsUnique()
                        .HasDatabaseName("ix_customers_customer_no");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("ix_customers_email");

                    b.HasIndex("NationalId")
                        .IsUnique()
                        .HasDatabaseName("ix_customers_national_id");

                    b.ToTable("customers", null, t =>
                        {
                            t.HasCheckConstraint("ck_customers_national_id", "national_id ~ '^[1-9][0-9]{10}$'");

                            t.HasCheckConstraint("ck_customers_status", "status IN ('Active', 'Passive')");
                        });
                });

            modelBuilder.Entity("FinancialTransactions.Domain.Users.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("now()");

                    b.Property<Guid?>("CustomerId")
                        .HasColumnType("uuid")
                        .HasColumnName("customer_id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(254)
                        .HasColumnType("character varying(254)")
                        .HasColumnName("email");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("password_hash");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("role");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasIndex("CustomerId")
                        .IsUnique()
                        .HasDatabaseName("ix_users_customer_id");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("ix_users_email");

                    b.ToTable("users", null, t =>
                        {
                            t.HasCheckConstraint("ck_users_customer_role", "(role = 'Customer') = (customer_id IS NOT NULL)");

                            t.HasCheckConstraint("ck_users_role", "role IN ('Admin', 'Customer')");
                        });
                });

            modelBuilder.Entity("FinancialTransactions.Domain.Accounts.Account", b =>
                {
                    b.HasOne("FinancialTransactions.Domain.Customers.Customer", null)
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_accounts_customers_customer_id");
                });

            modelBuilder.Entity("FinancialTransactions.Domain.Users.User", b =>
                {
                    b.HasOne("FinancialTransactions.Domain.Customers.Customer", null)
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_users_customers_customer_id");
                });
#pragma warning restore 612, 618
        }
    }
}
