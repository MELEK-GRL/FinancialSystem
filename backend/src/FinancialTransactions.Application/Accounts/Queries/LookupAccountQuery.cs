using FinancialTransactions.Application.Common.Validation;
using FinancialTransactions.Domain.Accounts;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Accounts.Queries;

public sealed record LookupAccountQuery(string AccountNo) : IRequest<AccountLookupDto>;

public sealed record AccountLookupDto(string AccountNo, string OwnerName, bool IsActive);

public sealed record AccountOwnerRow(string AccountNo, string FirstName, string LastName, bool AccountActive, bool CustomerActive);

public sealed class LookupAccountQueryValidator : AbstractValidator<LookupAccountQuery>
{
    public LookupAccountQueryValidator()
    {
        RuleFor(query => query.AccountNo).MustBeValidAccountNo();
    }
}

public sealed class LookupAccountQueryHandler(IAccountQueries accountQueries) : IRequestHandler<LookupAccountQuery, AccountLookupDto>
{
    public async Task<AccountLookupDto> Handle(LookupAccountQuery query, CancellationToken cancellationToken)
    {
        var owner = await accountQueries.GetOwnerByAccountNoAsync(AccountRules.NormalizeAccountNo(query.AccountNo), cancellationToken)
            ?? throw new AccountNotFoundException();

        return new AccountLookupDto(
            owner.AccountNo,
            NameMasker.Mask(owner.FirstName, owner.LastName),
            owner.AccountActive && owner.CustomerActive);
    }
}
