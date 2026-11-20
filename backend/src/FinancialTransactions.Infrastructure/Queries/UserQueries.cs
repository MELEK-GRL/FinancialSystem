using FinancialTransactions.Application.Auth.Queries.GetMe;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Domain.Users;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Queries;

public sealed class UserQueries(AppDbContext dbContext) : IUserQueries
{
    public Task<MeResult?> GetMeAsync(Guid userId, CancellationToken cancellationToken) =>
        (from user in dbContext.Users.AsNoTracking()
         join customer in dbContext.Customers.AsNoTracking() on user.CustomerId equals customer.Id into customers
         from customer in customers.DefaultIfEmpty()
         where user.Id == userId
         select new MeResult(
             user.Id,
             user.Email,
             user.Role == UserRole.Admin ? AppRoles.Admin : AppRoles.Customer,
             user.CustomerId,
             customer != null ? customer.CustomerNo : null,
             customer != null ? customer.FirstName : null,
             customer != null ? customer.LastName : null))
        .SingleOrDefaultAsync(cancellationToken);
}
