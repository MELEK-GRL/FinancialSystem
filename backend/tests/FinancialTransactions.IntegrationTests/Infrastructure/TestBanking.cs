using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FinancialTransactions.IntegrationTests.Infrastructure;

public static class TestBanking
{
    public static async Task<Guid> OpenAccountAsync(this TestCustomer customer)
    {
        var response = await customer.Client.PostAsync("/api/v1/accounts", null);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    public static async Task DepositAsync(this TestCustomer customer, Guid accountId, decimal amount)
    {
        var response = await customer.Client.PostAsJsonAsync("/api/v1/transactions/deposit", new { accountId, amount });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    public static async Task<decimal> BalanceAsync(this TestCustomer customer, Guid accountId) =>
        (await customer.Client.GetFromJsonAsync<JsonElement>($"/api/v1/accounts/{accountId}/balance"))
            .GetProperty("balance")
            .GetDecimal();
}
