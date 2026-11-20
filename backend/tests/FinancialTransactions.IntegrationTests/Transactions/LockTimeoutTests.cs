using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.Domain.Transactions;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Transactions;

public class LockTimeoutTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task LockedAccount_TimesOut_RecordsFailure_AndReplaysSameResult()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        await customer.DepositAsync(accountId, 100m);
        var key = Guid.NewGuid().ToString();

        HttpResponseMessage response;
        await using (var locker = factory.CreateDbContext())
        {
            await using var lockTransaction = await locker.Database.BeginTransactionAsync();
            await locker.Database.ExecuteSqlAsync($"SELECT 1 FROM accounts WHERE id = {accountId} FOR NO KEY UPDATE");

            response = await WithdrawAsync(customer, accountId, key, CancellationToken.None);
        }

        var retry = await WithdrawAsync(customer, accountId, key, CancellationToken.None);
        var transaction = await FindAsync(customer.CustomerId, key);

        await AssertLockTimeoutAsync(response);
        await AssertLockTimeoutAsync(retry);
        Assert.Equal(TransactionStatus.Failed, transaction!.Status);
        Assert.Equal(ErrorCodes.LockTimeout, transaction.FailureReason);
        Assert.Equal(100m, await customer.BalanceAsync(accountId));
    }

    [Fact]
    public async Task ClientCancellation_DoesNotLeaveTransactionPending()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        await customer.DepositAsync(accountId, 100m);
        var key = Guid.NewGuid().ToString();

        using var cancellation = new CancellationTokenSource();
        Task<HttpResponseMessage> request;

        await using (var locker = factory.CreateDbContext())
        {
            await using var lockTransaction = await locker.Database.BeginTransactionAsync();
            await locker.Database.ExecuteSqlAsync($"SELECT 1 FROM accounts WHERE id = {accountId} FOR NO KEY UPDATE");

            request = WithdrawAsync(customer, accountId, key, cancellation.Token);
            await Task.Delay(500);
            await cancellation.CancelAsync();
        }

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request);
        var transaction = await WaitUntilFinishedAsync(customer.CustomerId, key);
        var retry = await WithdrawAsync(customer, accountId, key, CancellationToken.None);

        Assert.Equal(TransactionStatus.Success, transaction.Status);
        Assert.Equal(HttpStatusCode.Created, retry.StatusCode);
        Assert.Equal(60m, await customer.BalanceAsync(accountId));
    }

    private static Task<HttpResponseMessage> WithdrawAsync(TestCustomer customer, Guid accountId, string key, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/transactions/withdraw")
        {
            Content = JsonContent.Create(new { accountId, amount = 40m }),
        };
        request.Headers.Add("Idempotency-Key", key);
        return customer.Client.SendAsync(request, cancellationToken);
    }

    private async Task<Transaction?> FindAsync(Guid customerId, string key)
    {
        await using var dbContext = factory.CreateDbContext();
        return await dbContext.Transactions.SingleOrDefaultAsync(t => t.InitiatedByCustomerId == customerId && t.IdempotencyKey == key);
    }

    private async Task<Transaction> WaitUntilFinishedAsync(Guid customerId, string key)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            if (await FindAsync(customerId, key) is { Status: not TransactionStatus.Pending } transaction)
            {
                return transaction;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException("Transaction is still pending");
    }

    private static async Task AssertLockTimeoutAsync(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(ErrorCodes.LockTimeout, problem.GetProperty("errorCode").GetString());
    }
}
