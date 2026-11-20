using System.Net;
using System.Security.Claims;
using FinancialTransactions.Api.Services;
using FinancialTransactions.Application.Common.Security;
using Microsoft.AspNetCore.Http;

namespace FinancialTransactions.IntegrationTests.Security;

public class CurrentUserTests
{
    [Fact]
    public void ReadsUserInformation_FromClaimsAndRequest()
    {
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(
            [
                new Claim(AppClaims.UserId, userId.ToString()),
                new Claim(AppClaims.CustomerId, customerId.ToString()),
                new Claim(AppClaims.Role, AppRoles.Customer),
            ]);

        Assert.Equal(userId, currentUser.UserId);
        Assert.Equal(customerId, currentUser.CustomerId);
        Assert.False(currentUser.IsAdmin);
        Assert.Equal("corr-1", currentUser.CorrelationId);
        Assert.Equal("10.0.0.7", currentUser.IpAddress);
    }

    [Fact]
    public void IsAdmin_IsTrue_ForAdminRole()
    {
        var currentUser = CreateCurrentUser([new Claim(AppClaims.Role, AppRoles.Admin)]);

        Assert.True(currentUser.IsAdmin);
        Assert.Null(currentUser.CustomerId);
    }

    [Fact]
    public void Anonymous_HasNoUserOrCustomer()
    {
        var currentUser = CreateCurrentUser([]);

        Assert.Null(currentUser.UserId);
        Assert.Null(currentUser.CustomerId);
        Assert.False(currentUser.IsAdmin);
    }

    private static CurrentUser CreateCurrentUser(Claim[] claims)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, claims.Length > 0 ? "Test" : null)),
            TraceIdentifier = "corr-1",
        };
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.7");

        return new CurrentUser(new HttpContextAccessor { HttpContext = httpContext });
    }
}
