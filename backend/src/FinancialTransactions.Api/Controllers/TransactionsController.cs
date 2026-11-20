using FinancialTransactions.Api.Security;
using FinancialTransactions.Application.Transactions;
using FinancialTransactions.Application.Transactions.Commands.Deposit;
using FinancialTransactions.Application.Transactions.Commands.Withdraw;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactions.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
public sealed class TransactionsController(ISender sender) : ApiControllerBase
{
    [HttpPost("deposit")]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<IActionResult> Deposit(DepositCommand command, CancellationToken cancellationToken)
    {
        var transaction = await sender.Send(command, cancellationToken);
        return Created($"/api/v1/transactions/{transaction.ReferenceNo}", transaction);
    }

    [HttpPost("withdraw")]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<IActionResult> Withdraw(WithdrawCommand command, CancellationToken cancellationToken)
    {
        var transaction = await sender.Send(command, cancellationToken);
        return Created($"/api/v1/transactions/{transaction.ReferenceNo}", transaction);
    }
}
