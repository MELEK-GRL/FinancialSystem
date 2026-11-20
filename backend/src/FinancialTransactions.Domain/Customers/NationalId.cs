namespace FinancialTransactions.Domain.Customers;

public static class NationalId
{
    public const int Length = 11;

    public static bool IsValid(string? value)
    {
        if (value is null || value.Length != Length || !value.All(char.IsAsciiDigit) || value[0] == '0')
        {
            return false;
        }

        var digits = value.Select(character => character - '0').ToArray();
        var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
        var tenthDigit = ((oddSum * 7 - evenSum) % 10 + 10) % 10;
        var eleventhDigit = digits.Take(10).Sum() % 10;

        return digits[9] == tenthDigit && digits[10] == eleventhDigit;
    }
}
