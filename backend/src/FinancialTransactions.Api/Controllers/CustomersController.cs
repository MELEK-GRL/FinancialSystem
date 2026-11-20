using FinancialTransactions.Api.Security;
using FinancialTransactions.Application.Common.Paging;
using FinancialTransactions.Application.Customers;
using FinancialTransactions.Application.Customers.Commands;
using FinancialTransactions.Application.Customers.Queries;
using FinancialTransactions.Domain.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTransactions.Api.Controllers;

public sealed class CustomersController(ISender sender) : ApiControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpGet("me")]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
    public Task<CustomerDto> GetMyProfile(CancellationToken cancellationToken) =>
        sender.Send(new GetMyProfileQuery(), cancellationToken);

    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpPatch("me")]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public Task<CustomerDto> UpdateMyProfile(UpdateMyProfileCommand command, CancellationToken cancellationToken) =>
        sender.Send(command, cancellationToken);

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet]
    [ProducesResponseType<PagedResult<CustomerSummaryDto>>(StatusCodes.Status200OK)]
    public Task<PagedResult<CustomerSummaryDto>> List(
        [FromQuery] string? search,
        [FromQuery] CustomerStatus? status,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int size = PageRequest.DefaultSize) =>
        sender.Send(new ListCustomersQuery(search, status, page, size), cancellationToken);

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<CustomerDto> Get(Guid id, CancellationToken cancellationToken) =>
        sender.Send(new GetCustomerQuery(id), cancellationToken);

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<CustomerDto> ChangeStatus(Guid id, ChangeStatusRequest request, CancellationToken cancellationToken) =>
        sender.Send(new ChangeCustomerStatusCommand(id, request.Status), cancellationToken);
}

public sealed record ChangeStatusRequest(CustomerStatus Status);
