namespace FinancialTransactions.IntegrationTests.Infrastructure;

public static class TestData
{
    public static string UniqueEmail() => $"{Guid.NewGuid():N}@example.com";

    public static string ValidNationalId()
    {
        var digits = new int[11];
        digits[0] = Random.Shared.Next(1, 10);
        for (var index = 1; index < 9; index++)
        {
            digits[index] = Random.Shared.Next(0, 10);
        }

        var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
        digits[9] = ((oddSum * 7 - evenSum) % 10 + 10) % 10;
        digits[10] = digits.Take(10).Sum() % 10;

        return string.Concat(digits);
    }
}
