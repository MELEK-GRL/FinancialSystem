using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialTransactions.Infrastructure.Persistence.Migrations
{
    public partial class EnforceAppendOnlyTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE FUNCTION enforce_transaction_append_only() RETURNS trigger AS $$
                BEGIN
                    IF TG_OP IN ('DELETE', 'TRUNCATE') THEN
                        RAISE EXCEPTION 'transactions is append-only: % is not allowed', TG_OP;
                    END IF;

                    IF OLD.status <> 'Pending' THEN
                        RAISE EXCEPTION 'transaction % is already % and cannot be changed', OLD.reference_no, OLD.status;
                    END IF;

                    IF (NEW.id, NEW.reference_no, NEW.type, NEW.amount, NEW.currency, NEW.source_account_id,
                        NEW.target_account_id, NEW.description, NEW.idempotency_key, NEW.initiated_by_customer_id, NEW.created_at)
                       IS DISTINCT FROM
                       (OLD.id, OLD.reference_no, OLD.type, OLD.amount, OLD.currency, OLD.source_account_id,
                        OLD.target_account_id, OLD.description, OLD.idempotency_key, OLD.initiated_by_customer_id, OLD.created_at) THEN
                        RAISE EXCEPTION 'only status and failure_reason of transaction % can change', OLD.reference_no;
                    END IF;

                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_transactions_append_only
                    BEFORE UPDATE OR DELETE ON transactions
                    FOR EACH ROW EXECUTE FUNCTION enforce_transaction_append_only();

                CREATE TRIGGER trg_transactions_no_truncate
                    BEFORE TRUNCATE ON transactions
                    FOR EACH STATEMENT EXECUTE FUNCTION enforce_transaction_append_only();
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER trg_transactions_no_truncate ON transactions;
                DROP TRIGGER trg_transactions_append_only ON transactions;
                DROP FUNCTION enforce_transaction_append_only();
                """);
        }
    }
}
