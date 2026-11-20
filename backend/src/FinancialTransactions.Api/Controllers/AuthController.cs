using FinancialTransactions.Application.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactions.Api.Controllers;

public sealed class AuthController(ISender sender) : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<RegisterResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Created("/api/v1/customers/me", result);
    }
}
