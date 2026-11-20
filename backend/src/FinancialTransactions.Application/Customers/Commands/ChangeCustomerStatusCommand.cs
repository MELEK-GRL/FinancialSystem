using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Domain.Customers;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Customers.Commands;

public sealed record ChangeCustomerStatusCommand(Guid CustomerId, CustomerStatus Status) : IRequest<CustomerDto>;

public sealed class ChangeCustomerStatusCommandValidator : AbstractValidator<ChangeCustomerStatusCommand>
{
    public ChangeCustomerStatusCommandValidator()
    {
        RuleFor(command => command.Status).IsInEnum();
    }
}

public sealed class ChangeCustomerStatusCommandHandler(ICustomerRepository customers, IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeCustomerStatusCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(ChangeCustomerStatusCommand command, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(command.CustomerId, cancellationToken) ?? throw new CustomerNotFoundException();

        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            if (command.Status == CustomerStatus.Active)
            {
                customer.Activate();
            }
            else
            {
                customer.Deactivate();
            }

            return Task.CompletedTask;
        }, cancellationToken);

        return CustomerDto.From(customer);
    }
}
