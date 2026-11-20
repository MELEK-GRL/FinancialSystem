using FinancialTransactions.Application.Transactions.Queries;
using FinancialTransactions.Domain.Transactions;

namespace FinancialTransactions.UnitTests.Transactions;

public class GetAccountHistoryQueryValidatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 20);
    private readonly GetAccountHistoryQueryValidator validator = new();

    [Fact]
    public void NoFilters_And_FullFilters_Pass()
    {
        Assert.True(validator.Validate(new GetAccountHistoryQuery(Guid.NewGuid())).IsValid);
        Assert.True(validator.Validate(new GetAccountHistoryQuery(
            Guid.NewGuid(), Today, Today, TransactionType.Transfer, TransactionStatus.Failed, 10m, 10m)).IsValid);
    }

    [Fact]
    public void ToBeforeFrom_Fails()
    {
        var result = validator.Validate(new GetAccountHistoryQuery(Guid.NewGuid(), From: Today, To: Today.AddDays(-1)));

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(GetAccountHistoryQuery.To), error.PropertyName);
        Assert.Equal("Bitiş tarihi başlangıç tarihinden önce olamaz", error.ErrorMessage);
    }

    [Fact]
    public void MaxBelowMin_Fails()
    {
        var result = validator.Validate(new GetAccountHistoryQuery(Guid.NewGuid(), MinAmount: 100m, MaxAmount: 50m));

        Assert.Equal(nameof(GetAccountHistoryQuery.MaxAmount), Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("10.005")]
    public void InvalidMinAmount_Fails(string amount)
    {
        var result = validator.Validate(new GetAccountHistoryQuery(Guid.NewGuid(), MinAmount: decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture)));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetAccountHistoryQuery.MinAmount));
    }

    [Fact]
    public void UnknownType_Fails()
    {
        Assert.False(validator.Validate(new GetAccountHistoryQuery(Guid.NewGuid(), Type: (TransactionType)99)).IsValid);
    }
}
