using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Application.Common.Interfaces;

namespace FinancialTransactions.Application.Common.Security;

public static class CurrentUserExtensions
{
    public static bool CanAccessCustomer(this ICurrentUser currentUser, Guid ownerCustomerId) =>
        currentUser.IsAdmin || currentUser.CustomerId == ownerCustomerId;

    public static Guid RequireCustomerId(this ICurrentUser currentUser) =>
        currentUser.CustomerId ?? throw new ForbiddenAccessException();
}
