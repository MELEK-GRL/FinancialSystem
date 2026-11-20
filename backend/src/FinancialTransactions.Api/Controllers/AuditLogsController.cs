using FinancialTransactions.Api.Security;
using FinancialTransactions.Application.Audit.Queries;
using FinancialTransactions.Application.Common.Paging;
using FinancialTransactions.Domain.Audit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactions.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public sealed class AuditLogsController(ISender sender) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<AuditLogDto>>(StatusCodes.Status200OK)]
    public Task<PagedResult<AuditLogDto>> List(
        [FromQuery] Guid? accountId,
        [FromQuery] AuditAction? action,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int size = PageRequest.DefaultSize) =>
        sender.Send(new ListAuditLogsQuery(accountId, action, from, to, page, size), cancellationToken);
}
