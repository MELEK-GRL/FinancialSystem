namespace FinancialTransactions.Application.Auth.Queries.GetMe;

public interface IUserQueries
{
    Task<MeResult?> GetMeAsync(Guid userId, CancellationToken cancellationToken);
}
