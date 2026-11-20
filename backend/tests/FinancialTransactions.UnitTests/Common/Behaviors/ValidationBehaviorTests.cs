using FinancialTransactions.Application.Common.Behaviors;
using FinancialTransactions.Application.Common.Exceptions;
using FluentValidation;

namespace FinancialTransactions.UnitTests.Common.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_InvalidRequest_ThrowsWithCamelCaseFieldErrors_AndSkipsHandler()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([new SampleRequestValidator()]);
        var handlerCalled = false;

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() =>
            behavior.Handle(new SampleRequest("", -5), _ =>
            {
                handlerCalled = true;
                return Task.FromResult("ok");
            }, CancellationToken.None));

        Assert.False(handlerCalled);
        Assert.Equal(["name", "amount"], exception.Errors.Keys);
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsHandler()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([new SampleRequestValidator()]);

        var result = await behavior.Handle(new SampleRequest("Ayşe", 10), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_NoValidators_CallsHandler()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([]);

        var result = await behavior.Handle(new SampleRequest("", -5), _ => Task.FromResult("ok"), CancellationToken.None);

        Assert.Equal("ok", result);
    }

    public sealed record SampleRequest(string Name, decimal Amount);

    private sealed class SampleRequestValidator : AbstractValidator<SampleRequest>
    {
        public SampleRequestValidator()
        {
            RuleFor(request => request.Name).NotEmpty();
            RuleFor(request => request.Amount).GreaterThan(0);
        }
    }
}
