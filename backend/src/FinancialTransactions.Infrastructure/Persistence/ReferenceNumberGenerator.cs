using FinancialTransactions.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Persistence;

public sealed class ReferenceNumberGenerator(AppDbContext dbContext) : IReferenceNumberGenerator
{
    public const string Sequence = "transaction_reference_seq";

    public Task<string> NextAsync(CancellationToken cancellationToken) =>
        dbContext.Database
            .SqlQueryRaw<string>($"""
                SELECT 'TX'
                    || to_char(now() AT TIME ZONE 'Europe/Istanbul', 'YYYYMMDD')
                    || lpad(nextval('{Sequence}')::text, 8, '0') AS "Value"
                """)
            .SingleAsync(cancellationToken);
}
