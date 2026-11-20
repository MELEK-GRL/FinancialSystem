using FinancialTransactions.Domain.Customers;

namespace FinancialTransactions.UnitTests.Domain;

public class NationalIdTests
{
    [Theory]
    [InlineData("10000000146")]
    [InlineData("12345678950")]
    [InlineData("98765432150")]
    public void ValidNationalIds_Pass(string value)
    {
        Assert.True(NationalId.IsValid(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234567895")]
    [InlineData("123456789501")]
    [InlineData("02345678950")]
    [InlineData("12345678951")]
    [InlineData("12345678940")]
    [InlineData("1234567895A")]
    public void InvalidNationalIds_Fail(string? value)
    {
        Assert.False(NationalId.IsValid(value));
    }
}
