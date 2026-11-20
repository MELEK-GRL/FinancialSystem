using FinancialTransactions.Application.Reports.Queries;
using NSubstitute;

namespace FinancialTransactions.UnitTests.Reports;

public class GetSummaryQueryHandlerTests
{
    private readonly IReportQueries reportQueries = Substitute.For<IReportQueries>();

    [Fact]
    public async Task Summary_CalculatesNetFlow_AndUsesIstanbulDays()
    {
        reportQueries.GetSummaryAsync(Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new SummaryTotals(1000m, 250.50m, 3, 2));
        var handler = new GetSummaryQueryHandler(reportQueries);

        var result = await handler.Handle(new GetSummaryQuery(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)), CancellationToken.None);

        Assert.Equal(749.50m, result.NetFlow);
        Assert.Equal("TRY", result.Currency);
        await reportQueries.Received(1).GetSummaryAsync(
            new DateTimeOffset(2026, 10, 31, 21, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 11, 30, 21, 0, 0, TimeSpan.Zero),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void EndBeforeStart_Fails()
    {
        var result = new GetSummaryQueryValidator().Validate(new GetSummaryQuery(new DateOnly(2026, 11, 20), new DateOnly(2026, 11, 19)));

        Assert.Equal("Bitiş tarihi başlangıç tarihinden önce olamaz", Assert.Single(result.Errors).ErrorMessage);
    }
}
