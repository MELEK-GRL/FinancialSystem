using FinancialTransactions.Api.Security;
using FinancialTransactions.Application.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactions.Api.Controllers;

public sealed class ReportsController(ISender sender) : ApiControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("summary")]
    [ProducesResponseType<SummaryReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<SummaryReportDto> GetSummary([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken) =>
        sender.Send(new GetSummaryQuery(from, to), cancellationToken);

    [HttpGet("periodic")]
    [ProducesResponseType<PeriodicReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<PeriodicReportDto> GetPeriodic(
        [FromQuery] ReportPeriod? period,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken) =>
        sender.Send(new GetPeriodicReportQuery(period, from, to), cancellationToken);

    [HttpGet("accounts/{id:guid}")]
    [ProducesResponseType<AccountReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<AccountReportDto> GetAccountReport(
        Guid id,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken) =>
        sender.Send(new GetAccountReportQuery(id, from, to), cancellationToken);
}
