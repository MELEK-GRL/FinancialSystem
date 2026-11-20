using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialTransactions.Infrastructure.Persistence.Migrations
{
    public partial class AddAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "account_no_seq",
                startValue: 1000000001L);

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'TR' || nextval('account_no_seq')"),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false, defaultValue: "TRY"),
                    balance = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => x.id);
                    table.CheckConstraint("ck_accounts_balance_not_negative", "balance >= 0");
                    table.CheckConstraint("ck_accounts_currency", "currency ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("ck_accounts_status", "status IN ('Active', 'Passive')");
                    table.ForeignKey(
                        name: "fk_accounts_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                CREATE TRIGGER trg_accounts_updated_at
                    BEFORE UPDATE ON accounts
                    FOR EACH ROW EXECUTE FUNCTION set_updated_at();
                """);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_account_no",
                table: "accounts",
                column: "account_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_customer_id",
                table: "accounts",
                column: "customer_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropSequence(
                name: "account_no_seq");
        }
    }
}
