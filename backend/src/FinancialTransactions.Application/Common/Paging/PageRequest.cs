using FluentValidation;

namespace FinancialTransactions.Application.Common.Paging;

public sealed record PageRequest(int Page = 1, int Size = PageRequest.DefaultSize)
{
    public const int DefaultSize = 20;
    public const int MaxSize = 100;

    public int Skip => (Page - 1) * Size;
}

public sealed class PageRequestValidator : AbstractValidator<PageRequest>
{
    public PageRequestValidator()
    {
        RuleFor(request => request.Page).GreaterThanOrEqualTo(1);
        RuleFor(request => request.Size).InclusiveBetween(1, PageRequest.MaxSize);
    }
}
