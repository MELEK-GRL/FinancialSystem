using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialTransactions.Infrastructure.Persistence.Migrations
{
    public partial class AddTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_no = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    source_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    initiated_by_customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                    table.CheckConstraint("ck_transactions_accounts_match_type", "(type = 'Deposit' AND source_account_id IS NULL AND target_account_id IS NOT NULL)\nOR (type = 'Withdrawal' AND source_account_id IS NOT NULL AND target_account_id IS NULL)\nOR (type = 'Transfer' AND source_account_id IS NOT NULL AND target_account_id IS NOT NULL\n    AND source_account_id <> target_account_id)");
                    table.CheckConstraint("ck_transactions_amount_positive", "amount > 0");
                    table.CheckConstraint("ck_transactions_currency", "currency ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("ck_transactions_status", "status IN ('Pending', 'Success', 'Failed')");
                    table.CheckConstraint("ck_transactions_type", "type IN ('Deposit', 'Withdrawal', 'Transfer')");
                    table.ForeignKey(
                        name: "fk_transactions_accounts_source_account_id",
                        column: x => x.source_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_accounts_target_account_id",
                        column: x => x.target_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_customers_initiated_by_customer_id",
                        column: x => x.initiated_by_customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                CREATE FUNCTION set_transaction_completed_at() RETURNS trigger AS $$
                BEGIN
                    IF OLD.status = 'Pending' AND NEW.status <> 'Pending' THEN
                        NEW.completed_at = now();
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_transactions_completed_at
                    BEFORE UPDATE ON transactions
                    FOR EACH ROW EXECUTE FUNCTION set_transaction_completed_at();
                """);

            migrationBuilder.CreateIndex(
                name: "ix_transactions_created_at",
                table: "transactions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_initiated_by_customer_id_idempotency_key",
                table: "transactions",
                columns: new[] { "initiated_by_customer_id", "idempotency_key" },
                unique: true,
                filter: "idempotency_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_pending_created_at",
                table: "transactions",
                column: "created_at",
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_reference_no",
                table: "transactions",
                column: "reference_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transactions_source_account_id_created_at",
                table: "transactions",
                columns: new[] { "source_account_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_status_created_at",
                table: "transactions",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_target_account_id_created_at",
                table: "transactions",
                columns: new[] { "target_account_id", "created_at" },
                descending: new[] { false, true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.Sql("DROP FUNCTION set_transaction_completed_at();");
        }
    }
}
