using FinancialTransactions.Api.Security;
using FinancialTransactions.Application.Transactions;
using FinancialTransactions.Application.Transactions.Commands.Deposit;
using FinancialTransactions.Application.Transactions.Commands.Transfer;
using FinancialTransactions.Application.Transactions.Commands.Withdraw;
using FinancialTransactions.Application.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactions.Api.Controllers;

public sealed class TransactionsController(ISender sender) : ApiControllerBase
{
    public const string IdempotencyKeyHeader = "Idempotency-Key";

    [HttpGet("{referenceNo}")]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<TransactionDto> Get(string referenceNo, CancellationToken cancellationToken) =>
        sender.Send(new GetTransactionQuery(referenceNo), cancellationToken);

    [HttpPost("deposit")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable, "application/problem+json")]
    public Task<IActionResult> Deposit(
        DepositCommand command,
        [FromHeader(Name = IdempotencyKeyHeader)] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(command with { IdempotencyKey = idempotencyKey }, cancellationToken);

    [HttpPost("withdraw")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable, "application/problem+json")]
    public Task<IActionResult> Withdraw(
        WithdrawCommand command,
        [FromHeader(Name = IdempotencyKeyHeader)] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(command with { IdempotencyKey = idempotencyKey }, cancellationToken);

    [HttpPost("transfer")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable, "application/problem+json")]
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
