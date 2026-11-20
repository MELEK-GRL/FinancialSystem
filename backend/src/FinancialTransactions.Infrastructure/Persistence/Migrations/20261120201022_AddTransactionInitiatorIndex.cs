using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialTransactions.Infrastructure.Persistence.Migrations
{
    public partial class AddTransactionInitiatorIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_transactions_initiated_by_customer_id_created_at",
                table: "transactions",
                columns: new[] { "initiated_by_customer_id", "created_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_transactions_initiated_by_customer_id_created_at",
                table: "transactions");
        }
    }
}
