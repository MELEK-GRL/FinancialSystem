namespace FinancialTransactions.Application.Common.Security;

public static class AppClaims
{
    public const string UserId = "sub";
    public const string CustomerId = "customer_id";
    public const string Role = "role";
}

public static class AppRoles
{
    public const string Admin = "ADMIN";
    public const string Customer = "CUSTOMER";
}
