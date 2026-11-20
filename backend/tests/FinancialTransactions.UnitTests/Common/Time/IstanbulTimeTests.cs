using FinancialTransactions.Application.Common.Time;

namespace FinancialTransactions.UnitTests.Common.Time;

public class IstanbulTimeTests
{
    [Fact]
    public void StartOfDay_IsIstanbulMidnightInUtc()
    {
        var start = IstanbulTime.StartOfDay(new DateOnly(2026, 11, 20));

        Assert.Equal(new DateTimeOffset(2026, 11, 19, 21, 0, 0, TimeSpan.Zero), start);
        Assert.Equal(TimeSpan.Zero, start.Offset);
    }
}
