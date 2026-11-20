using FinancialTransactions.Application.Accounts;

namespace FinancialTransactions.UnitTests.Accounts;

public class NameMaskerTests
{
    [Theory]
    [InlineData("Ayşe", "Yılmaz", "Ay*** Yı***")]
    [InlineData("Ayşe Nur", "Kaya", "Ay*** Nu*** Ka***")]
    [InlineData("A", "Öz", "A*** Öz***")]
    [InlineData(" Mehmet ", " Demir ", "Me*** De***")]
    public void Mask_ShowsOnlyFirstTwoLettersOfEachName(string firstName, string lastName, string expected)
    {
        Assert.Equal(expected, NameMasker.Mask(firstName, lastName));
    }
}
