using FinancialTransactions.Application.Common.Exceptions;
using FinancialTransactions.Application.Common.Interfaces;
using FinancialTransactions.Application.Common.Security;
using FinancialTransactions.Application.Common.Validation;
using FinancialTransactions.Application.Users;
using FinancialTransactions.Domain.Customers;
using FluentValidation;
using MediatR;

namespace FinancialTransactions.Application.Customers.Commands;

public sealed record UpdateMyProfileCommand(string Email, string Phone) : IRequest<CustomerDto>;

public sealed class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(command => command.Email).MustBeValidEmail();
        RuleFor(command => command.Phone).MustBeValidPhone();
    }
}

public sealed class UpdateMyProfileCommandHandler(
    ICurrentUser currentUser,
    ICustomerRepository customers,
    IUserRepository users,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateMyProfileCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(UpdateMyProfileCommand command, CancellationToken cancellationToken)
    {
        var customerId = currentUser.RequireCustomerId();
        var customer = await customers.GetByIdAsync(customerId, cancellationToken) ?? throw new CustomerNotFoundException();
        var user = await users.GetByCustomerIdAsync(customerId, cancellationToken) ?? throw new CustomerNotFoundException();
        var email = command.Email.Trim().ToLowerInvariant();

        if (email != customer.Email
            && (await customers.EmailTakenByOtherAsync(email, customerId, cancellationToken)
                || await users.EmailExistsAsync(email, cancellationToken)))
        {
            throw new DuplicateCustomerException();
        }

        try
        {
            await unitOfWork.ExecuteInTransactionAsync(_ =>
            {
                customer.UpdateContact(email, command.Phone);
                user.ChangeEmail(email);
                return Task.CompletedTask;
            }, cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            throw new DuplicateCustomerException();
        }

        return CustomerDto.From(customer);
    }
}
