using System.Text.Json;
using Xunit;

public sealed class CostReportingTests
{
    [Fact]
    public void PreviousMonthWindowHandlesYearBoundary()
    {
        var (start, end) = MonthlyCostReportFunction.PreviousMonthWindow(
            new DateTimeOffset(2026, 1, 1, 0, 15, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2025, 12, 1), start);
        Assert.Equal(new DateOnly(2025, 12, 31), end);
    }

    [Fact]
    public void PreviousMonthWindowHandlesLeapYear()
    {
        var (start, end) = MonthlyCostReportFunction.PreviousMonthWindow(
            new DateTimeOffset(2024, 3, 1, 0, 15, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2024, 2, 1), start);
        Assert.Equal(new DateOnly(2024, 2, 29), end);
    }

    [Fact]
    public void ParseQueryResultSumsCostsInOneCurrency()
    {
        using var result = JsonDocument.Parse(
            """{"columns":[{"name":"PreTaxCost"},{"name":"Currency"}],"rows":[[1.25,"USD"],[2.75,"USD"]]}""");

        var (amount, currency) = MonthlyCostReportFunction.ParseQueryResult(
            result.RootElement);

        Assert.Equal(4m, amount);
        Assert.Equal("USD", currency);
    }

    [Fact]
    public void ParseQueryResultRejectsMixedCurrencies()
    {
        using var result = JsonDocument.Parse(
            """{"columns":[{"name":"PreTaxCost"},{"name":"Currency"}],"rows":[[1,"USD"],[2,"EUR"]]}""");

        var error = Assert.Throws<InvalidOperationException>(
            () => MonthlyCostReportFunction.ParseQueryResult(result.RootElement));

        Assert.Contains("multiple currencies", error.Message);
    }

    [Fact]
    public void ParseQueryResultRejectsEmptyRows()
    {
        using var result = JsonDocument.Parse("""{"columns":[],"rows":[]}""");

        Assert.Throws<InvalidOperationException>(
            () => MonthlyCostReportFunction.ParseQueryResult(result.RootElement));
    }
}
