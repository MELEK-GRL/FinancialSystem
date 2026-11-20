using System.Net;
using System.Net.Http.Json;
using FinancialTransactions.Contracts.Transactions;
using FinancialTransactions.Domain.Transactions;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Transactions;

public class ConcurrencyTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task HundredWithdrawalsOf10_From500_ExactlyFiftySucceed()
    {
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        await customer.DepositAsync(accountId, 500m);

        var responses = await Task.WhenAll(Enumerable.Range(0, 100).Select(_ =>
            customer.Client.PostAsJsonAsync("/api/v1/transactions/withdraw", new { accountId, amount = 10m })));

        Assert.Equal(50, responses.Count(response => response.StatusCode == HttpStatusCode.Created));
        Assert.Equal(50, responses.Count(response => response.StatusCode == HttpStatusCode.UnprocessableEntity));
        Assert.Equal(0m, await customer.BalanceAsync(accountId));
    }

    [Fact]
    public async Task HundredsOfOppositeTransfers_DoNotDeadlock_AndKeepTotal()
    {
        var first = await factory.RegisterCustomerAsync();
        var second = await factory.RegisterCustomerAsync();
        var a = await first.OpenAccountWithNumberAsync();
        var b = await second.OpenAccountWithNumberAsync();
        await first.DepositAsync(a.Id, 1000m);
        await second.DepositAsync(b.Id, 1000m);

        var requests = Enumerable.Range(0, 200).Select(i => i % 2 == 0
            ? TransferAsync(first, a.Id, b.AccountNo, 1m)
            : TransferAsync(second, b.Id, a.AccountNo, 1m));
        var responses = await Task.WhenAll(requests);

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));
        Assert.Equal(1000m, await first.BalanceAsync(a.Id));
        Assert.Equal(1000m, await second.BalanceAsync(b.Id));
    }

    [Fact]
    public async Task RandomTransfersBetweenFourAccounts_KeepTotal_AndQueueOneEventPerTransaction()
    {
        var customers = new List<TestCustomer>();
        var accounts = new List<(Guid Id, string AccountNo)>();

        for (var i = 0; i < 4; i++)
        {
            var customer = await factory.RegisterCustomerAsync();
            var account = await customer.OpenAccountWithNumberAsync();
            await customer.DepositAsync(account.Id, 100m);
            customers.Add(customer);
            accounts.Add(account);
        }

        var random = new Random(42);
        var requests = Enumerable.Range(0, 200).Select(_ =>
        {
            var from = random.Next(4);
            var to = (from + 1 + random.Next(3)) % 4;
            return TransferAsync(customers[from], accounts[from].Id, accounts[to].AccountNo, random.Next(1, 31));
        }).ToList();
        var responses = await Task.WhenAll(requests);

        var balances = await Task.WhenAll(customers.Select((customer, i) => customer.BalanceAsync(accounts[i].Id)));
        Assert.All(responses, response => Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Created, HttpStatusCode.UnprocessableEntity }));
        Assert.Equal(400m, balances.Sum());
        Assert.All(balances, balance => Assert.True(balance >= 0));

        var accountIds = accounts.Select(account => account.Id).ToArray();
        await using var dbContext = factory.CreateDbContext();
        var transfers = await dbContext.Transactions
            .Where(t => t.Type == TransactionType.Transfer && accountIds.Contains(t.SourceAccountId!.Value))
            .Select(t => new
            {
                t.Status,
                Events = dbContext.OutboxMessages.Where(m => m.AggregateId == t.Id).Select(m => m.EventType).ToList(),
            })
            .ToListAsync();

        Assert.Equal(200, transfers.Count);
        Assert.DoesNotContain(transfers, t => t.Status == TransactionStatus.Pending);
        Assert.All(transfers, t => Assert.Equal(
            [t.Status == TransactionStatus.Success ? TransactionEventTypes.Completed : TransactionEventTypes.Failed],
            t.Events));
    }

    private static Task<HttpResponseMessage> TransferAsync(TestCustomer sender, Guid sourceAccountId, string targetAccountNo, decimal amount) =>
        sender.Client.PostAsJsonAsync("/api/v1/transactions/transfer", new { sourceAccountId, targetAccountNo, amount });
}
