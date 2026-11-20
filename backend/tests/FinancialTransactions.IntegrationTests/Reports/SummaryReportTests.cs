using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.IntegrationTests.Infrastructure;

namespace FinancialTransactions.IntegrationTests.Reports;

public class SummaryReportTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Url = "/api/v1/reports/summary";

    [Fact]
    public async Task Summary_CountsOnlySuccessfulDepositsAndWithdrawals()
    {
        var admin = await factory.CreateAdminClientAsync();
        var customer = await factory.RegisterCustomerAsync();
        var receiver = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        var target = await receiver.OpenAccountWithNumberAsync();
        var before = await admin.GetFromJsonAsync<JsonElement>(Url);

        await customer.DepositAsync(accountId, 100m);
        await customer.Client.PostAsJsonAsync("/api/v1/transactions/withdraw", new { accountId, amount = 30.25m });
        await customer.Client.PostAsJsonAsync("/api/v1/transactions/withdraw", new { accountId, amount = 500m });
        await customer.Client.PostAsJsonAsync(
            "/api/v1/transactions/transfer", new { sourceAccountId = accountId, targetAccountNo = target.AccountNo, amount = 20m });
        var after = await admin.GetFromJsonAsync<JsonElement>(Url);

        Assert.Equal(100m, Difference(before, after, "totalInflow"));
        Assert.Equal(30.25m, Difference(before, after, "totalOutflow"));
        Assert.Equal(69.75m, Difference(before, after, "netFlow"));
        Assert.Equal(1m, Difference(before, after, "depositCount"));
        Assert.Equal(1m, Difference(before, after, "withdrawalCount"));
        Assert.Equal("TRY", after.GetProperty("currency").GetString());
    }

    [Fact]
    public async Task DateFilter_ExcludesOtherDays()
    {
        var admin = await factory.CreateAdminClientAsync();
        var customer = await factory.RegisterCustomerAsync();
        var accountId = await customer.OpenAccountAsync();
        await customer.DepositAsync(accountId, 10m);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul")).DateTime);

        var todays = await admin.GetFromJsonAsync<JsonElement>($"{Url}?from={today:yyyy-MM-dd}&to={today:yyyy-MM-dd}");
        var tomorrows = await admin.GetFromJsonAsync<JsonElement>($"{Url}?from={today.AddDays(1):yyyy-MM-dd}");
        var yesterdays = await admin.GetFromJsonAsync<JsonElement>($"{Url}?to={today.AddDays(-1):yyyy-MM-dd}");

        Assert.True(todays.GetProperty("totalInflow").GetDecimal() >= 10m);
        Assert.Equal(today.ToString("yyyy-MM-dd"), todays.GetProperty("from").GetString());
        Assert.Equal(0m, tomorrows.GetProperty("totalInflow").GetDecimal());
        Assert.Equal(0, tomorrows.GetProperty("depositCount").GetInt64());
        Assert.Equal(0m, yesterdays.GetProperty("totalInflow").GetDecimal());
    }

    [Fact]
    public async Task Customer_Gets403_AndAnonymousGets401()
    {
        var customer = await factory.RegisterCustomerAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await customer.Client.GetAsync(Url)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync(Url)).StatusCode);
    }

    [Fact]
    public async Task EndBeforeStart_Returns400()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.GetAsync($"{Url}?from=2026-11-20&to=2026-11-19");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ErrorCodes.ValidationError, problem.GetProperty("errorCode").GetString());
    }

    private static decimal Difference(JsonElement before, JsonElement after, string property) =>
        after.GetProperty(property).GetDecimal() - before.GetProperty(property).GetDecimal();
}
