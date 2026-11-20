using System.Globalization;
using System.Runtime.CompilerServices;

namespace FinancialTransactions.UnitTests;

internal static class TestCulture
{
    [ModuleInitializer]
    internal static void UseTurkishMessages() =>
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("tr");
}
