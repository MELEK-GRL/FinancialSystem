namespace FinancialTransactions.Application.Accounts;

public static class NameMasker
{
    private const int VisibleLetters = 2;
    private const string MaskSuffix = "***";

    public static string Mask(string firstName, string lastName) =>
        string.Join(' ', $"{firstName} {lastName}"
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(MaskWord));

    private static string MaskWord(string word) => word[..Math.Min(VisibleLetters, word.Length)] + MaskSuffix;
}
