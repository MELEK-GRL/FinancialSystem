using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.IntegrationTests.Auth;

public class RegisterTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Url = "/api/v1/auth/register";
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Register_CreatesCustomerAndUserWithHashedPassword()
    {
        var request = NewRequest();

        var response = await client.PostAsJsonAsync(Url, request);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/api/v1/customers/me", response.Headers.Location?.OriginalString);
        Assert.Matches("^C[0-9]{9}$", body.GetProperty("customerNo").GetString());

        await using var dbContext = factory.CreateDbContext();
        var user = await dbContext.Users.SingleAsync(u => u.Email == request.Email);
        Assert.StartsWith("$2", user.PasswordHash);
        Assert.NotEqual(request.Password, user.PasswordHash);
        Assert.Equal(body.GetProperty("customerId").GetGuid(), user.CustomerId);
    }

    [Fact]
    public async Task Register_WithExistingNationalId_Returns409()
    {
        var first = NewRequest();
        await client.PostAsJsonAsync(Url, first);

        var response = await client.PostAsJsonAsync(Url, NewRequest() with { NationalId = first.NationalId });

        await AssertDuplicateAsync(response);
    }

    [Fact]
    public async Task Register_WithExistingEmail_Returns409()
    {
        var first = NewRequest();
        await client.PostAsJsonAsync(Url, first);

        var response = await client.PostAsJsonAsync(Url, NewRequest() with { Email = first.Email.ToUpperInvariant() });

        await AssertDuplicateAsync(response);
    }

    [Fact]
    public async Task Register_WithInvalidFields_Returns400WithFieldErrors()
    {
        var response = await client.PostAsJsonAsync(Url, NewRequest() with
        {
            NationalId = "12345678951",
            Phone = "123",
            Password = "weak",
        });
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(errors.TryGetProperty("nationalId", out _));
        Assert.True(errors.TryGetProperty("phone", out _));
        Assert.True(errors.TryGetProperty("password", out _));
    }

    [Fact]
    public async Task ConcurrentRegistrations_WithSameNationalId_CreateOnlyOneCustomer()
    {
        var nationalId = TestData.ValidNationalId();

        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            client.PostAsJsonAsync(Url, NewRequest() with { NationalId = nationalId })));

        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Created));
        Assert.Equal(4, responses.Count(response => response.StatusCode == HttpStatusCode.Conflict));

        await using var dbContext = factory.CreateDbContext();
        Assert.Equal(1, await dbContext.Customers.CountAsync(customer => customer.NationalId == nationalId));
    }

    private static async Task AssertDuplicateAsync(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ErrorCodes.DuplicateCustomer, problem.GetProperty("errorCode").GetString());
    }

    private static RegisterRequest NewRequest() =>
        new("Ayşe", "Yılmaz", TestData.ValidNationalId(), TestData.UniqueEmail(), "5321234567", "Sifre1234");

    private sealed record RegisterRequest(
        string FirstName,
        string LastName,
        string NationalId,
        string Email,
        string Phone,
        string Password);
}
