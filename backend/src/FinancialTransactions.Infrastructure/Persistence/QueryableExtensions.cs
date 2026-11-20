using FinancialTransactions.Application.Common.Paging;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Persistence;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PageRequest page,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(page.Skip).Take(page.Size).ToListAsync(cancellationToken);

        return new PagedResult<T>(items, page.Page, page.Size, totalCount);
    }
}
