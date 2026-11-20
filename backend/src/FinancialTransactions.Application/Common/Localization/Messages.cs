using System.Globalization;
using System.Resources;

namespace FinancialTransactions.Application.Common.Localization;

public sealed class Messages
{
    public const string AmountMustBePositive = "AMOUNT_MUST_BE_POSITIVE";
    public const string AmountTooManyDecimals = "AMOUNT_TOO_MANY_DECIMALS";

    private static readonly ResourceManager Resources = new(typeof(Messages));

    public static string Get(string key) => Resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;
}
