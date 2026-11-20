using System.Security.Claims;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;

namespace FinancialTransactions.Api.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private HttpContext? HttpContext => httpContextAccessor.HttpContext;

    private ClaimsPrincipal? Principal => HttpContext?.User;

    public Guid? UserId => ReadGuid(AppClaims.UserId);

    public Guid? CustomerId => ReadGuid(AppClaims.CustomerId);

    public bool IsAdmin => Principal?.HasClaim(AppClaims.Role, AppRoles.Admin) ?? false;

    public string CorrelationId => HttpContext?.TraceIdentifier ?? string.Empty;

    public string? IpAddress => HttpContext?.Connection.RemoteIpAddress?.ToString();

    private Guid? ReadGuid(string claimType) =>
        Guid.TryParse(Principal?.FindFirstValue(claimType), out var value) ? value : null;
}
