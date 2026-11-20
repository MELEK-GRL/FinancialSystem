using FinancialTransactions.Api.Security;
using FinancialTransactions.Application.Accounts;
using FinancialTransactions.Application.Accounts.Commands.OpenAccount;
using FinancialTransactions.Application.Accounts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactions.Api.Controllers;

public sealed class AccountsController(ISender sender) : ApiControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpPost]
    [ProducesResponseType<AccountDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<IActionResult> Open(CancellationToken cancellationToken)
    {
        var account = await sender.Send(new OpenAccountCommand(), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = account.Id }, account);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AccountDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<AccountDto>> List([FromQuery] Guid? customerId, CancellationToken cancellationToken) =>
        sender.Send(new ListAccountsQuery(customerId), cancellationToken);

    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpGet("lookup")]
    [ProducesResponseType<AccountLookupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<AccountLookupDto> Lookup([FromQuery] string accountNo, CancellationToken cancellationToken) =>
        sender.Send(new LookupAccountQuery(accountNo), cancellationToken);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<AccountDto> Get(Guid id, CancellationToken cancellationToken) =>
        sender.Send(new GetAccountQuery(id), cancellationToken);

    [HttpGet("{id:guid}/balance")]
    [ProducesResponseType<AccountBalanceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<AccountBalanceDto> GetBalance(Guid id, CancellationToken cancellationToken) =>
        sender.Send(new GetAccountBalanceQuery(id), cancellationToken);
}
