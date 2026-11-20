using FinancialTransactions.Application.Common.Paging;

namespace FinancialTransactions.UnitTests.Common.Paging;

public class PagingTests
{
    private readonly PageRequestValidator validator = new();

    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(3, 20, 40)]
    [InlineData(2, 50, 50)]
    public void Skip_IsCalculatedFromPageAndSize(int page, int size, int expectedSkip)
    {
        Assert.Equal(expectedSkip, new PageRequest(page, size).Skip);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 100)]
    public void ValidPageRequest_Passes(int page, int size)
    {
        Assert.True(validator.Validate(new PageRequest(page, size)).IsValid);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void InvalidPageRequest_Fails(int page, int size)
    {
        Assert.False(validator.Validate(new PageRequest(page, size)).IsValid);
    }

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(45, 20, 3)]
    [InlineData(40, 20, 2)]
    public void TotalPages_RoundsUp(int totalCount, int size, int expectedPages)
    {
        Assert.Equal(expectedPages, new PagedResult<int>([], 1, size, totalCount).TotalPages);
    }
}
