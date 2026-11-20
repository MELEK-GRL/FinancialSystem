using FinancialTransactions.Api.Security;
using FinancialTransactions.Application.Transactions;
using FinancialTransactions.Application.Transactions.Commands.Deposit;
using FinancialTransactions.Application.Transactions.Commands.Transfer;
using FinancialTransactions.Application.Transactions.Commands.Withdraw;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactions.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
[ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
public sealed class TransactionsController(ISender sender) : ApiControllerBase
{
    public const string IdempotencyKeyHeader = "Idempotency-Key";

    [HttpPost("deposit")]
    public Task<IActionResult> Deposit(
        DepositCommand command,
        [FromHeader(Name = IdempotencyKeyHeader)] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(command with { IdempotencyKey = idempotencyKey }, cancellationToken);

    [HttpPost("withdraw")]
    public Task<IActionResult> Withdraw(
        WithdrawCommand command,
        [FromHeader(Name = IdempotencyKeyHeader)] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(command with { IdempotencyKey = idempotencyKey }, cancellationToken);

    [HttpPost("transfer")]
    public Task<IActionResult> Transfer(
        TransferCommand command,
        [FromHeader(Name = IdempotencyKeyHeader)] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(command with { IdempotencyKey = idempotencyKey }, cancellationToken);

    private async Task<IActionResult> ExecuteAsync(IRequest<TransactionDto> command, CancellationToken cancellationToken)
    {
        var transaction = await sender.Send(command, cancellationToken);
        return Created($"/api/v1/transactions/{transaction.ReferenceNo}", transaction);
    }
}
