namespace FinancialTransactions.Application.Common.Time;

public static class IstanbulTime
{
    private static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

    public static DateTimeOffset StartOfDay(DateOnly date)
    {
        var localMidnight = date.ToDateTime(TimeOnly.MinValue);
        return new DateTimeOffset(localMidnight, TimeZone.GetUtcOffset(localMidnight)).ToUniversalTime();
    }
}
