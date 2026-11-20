using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Domain.Common;
using FinancialTransactions.Domain.Users;
using FinancialTransactions.Infrastructure.Options;
using FinancialTransactions.Infrastructure.Security;
using FinancialTransactions.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FinancialTransactions.IntegrationTests.Auth;

public class LoginTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Password = "Sifre1234";
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Login_ReturnsTokenWithUserClaims()
    {
        var (email, customerId) = await RegisterAsync();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = email.ToUpperInvariant(), password = Password });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = new JsonWebTokenHandler().ReadJsonWebToken(body.GetProperty("accessToken").GetString());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(AppRoles.Customer, body.GetProperty("role").GetString());
        Assert.Equal(AppRoles.Customer, token.GetClaim(AppClaims.Role).Value);
        Assert.Equal(customerId.ToString(), token.GetClaim(AppClaims.CustomerId).Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Login_WithWrongPasswordOrUnknownEmail_ReturnsSame401(bool knownEmail)
    {
        var email = knownEmail ? (await RegisterAsync()).Email : TestData.UniqueEmail();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Yanlis1234" });
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(ErrorCodes.InvalidCredentials, problem.GetProperty("errorCode").GetString());
        Assert.Equal("E-posta veya şifre hatalı", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Me_WithToken_ReturnsCurrentCustomer()
    {
        var (email, customerId) = await RegisterAsync();
        var token = await LoginAsync(email);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var me = await (await client.SendAsync(request)).Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(email, me.GetProperty("email").GetString());
        Assert.Equal(customerId, me.GetProperty("customerId").GetGuid());
        Assert.Equal("Ayşe", me.GetProperty("firstName").GetString());
    }

    [Fact]
    public async Task Me_ForAdmin_ReturnsNoCustomerInformation()
    {
        var jwtService = factory.Services.GetRequiredService<FinancialTransactions.Application.Common.Interfaces.IJwtTokenService>();
        var admin = User.CreateAdmin(TestData.UniqueEmail(), "hash");
        await using (var dbContext = factory.CreateDbContext())
        {
            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync();
        }

        var me = await (await SendMeAsync(jwtService.Create(admin).Token)).Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(AppRoles.Admin, me.GetProperty("role").GetString());
        Assert.Equal(JsonValueKind.Null, me.GetProperty("customerId").ValueKind);
        Assert.Equal(JsonValueKind.Null, me.GetProperty("customerNo").ValueKind);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401ProblemDetails()
    {
        var response = await client.GetAsync("/api/v1/auth/me");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(ErrorCodes.Unauthorized, problem.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Me_WithTamperedToken_Returns401()
    {
        var token = await LoginAsync((await RegisterAsync()).Email);

        var response = await SendMeAsync(token[..^4] + "abcd");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithExpiredToken_Returns401()
    {
        var jwtOptions = factory.Services.GetRequiredService<IOptions<JwtOptions>>();
        var twoHoursAgo = new FakeTimeProvider(DateTimeOffset.UtcNow.AddHours(-2));
        var expiredToken = new JwtTokenService(jwtOptions, twoHoursAgo)
            .Create(User.CreateAdmin("admin@example.com", "hash")).Token;

        var response = await SendMeAsync(expiredToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<(string Email, Guid CustomerId)> RegisterAsync()
    {
        var email = TestData.UniqueEmail();
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "Ayşe",
            lastName = "Yılmaz",
            nationalId = TestData.ValidNationalId(),
            email,
            phone = "5321234567",
            password = Password,
        });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (email, body.GetProperty("customerId").GetGuid());
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = Password });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    private Task<HttpResponseMessage> SendMeAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client.SendAsync(request);
    }
}
