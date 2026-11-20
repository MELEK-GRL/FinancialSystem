using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Domain.Users;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialTransactions.IntegrationTests.Infrastructure;

public static class TestAuth
{
    public const string Password = "Sifre1234";

    public static async Task<TestCustomer> RegisterCustomerAsync(this ApiFactory factory)
    {
        var client = factory.CreateClient();
        var email = TestData.UniqueEmail();

        var registration = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "Ayşe",
            lastName = "Yılmaz",
            nationalId = TestData.ValidNationalId(),
            email,
            phone = "5321234567",
            password = Password,
        });
        var customerId = (await registration.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("customerId").GetGuid();

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = Password });
        var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString()!;

        return new TestCustomer(customerId, email, factory.CreateAuthorizedClient(token));
    }

    public static async Task<HttpClient> CreateAdminClientAsync(this ApiFactory factory)
    {
        var admin = User.CreateAdmin(TestData.UniqueEmail(), "not-used");
        await using (var dbContext = factory.CreateDbContext())
        {
            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync();
        }

        var token = factory.Services.GetRequiredService<IJwtTokenService>().Create(admin).Token;
        return factory.CreateAuthorizedClient(token);
    }

    public static HttpClient CreateAuthorizedClient(this ApiFactory factory, string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}

public sealed record TestCustomer(Guid CustomerId, string Email, HttpClient Client);
