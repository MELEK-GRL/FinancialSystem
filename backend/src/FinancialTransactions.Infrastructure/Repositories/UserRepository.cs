using FinancialTransactions.Application.Users;
using FinancialTransactions.Domain.Users;
using FinancialTransactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);
}
