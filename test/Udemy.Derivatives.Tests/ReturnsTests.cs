using System.Text;

namespace Udemy.Derivatives.Tests;

[Trait("Category", "Unit")]
public class ReturnsTests(ITestOutputHelper output)
{
    [Fact]
    public void GetReturns_DailyAndWeekly_Observe()
    {
        double[] prices = [105, 111, 102, 109, 107, 108];
        var dailyReturns = prices.Skip(1).Zip(prices.SkipLast(1), (p, pm1) => Math.Log(p / pm1)).ToArray();
        var weeklyReturns = dailyReturns.Sum();

        output.WriteLine(
            $"""
             prices {string.Join(", ", prices)}
             daily returns {string.Join(", ", dailyReturns.Select(r => $"{r:P4}"))}
             weekly return {weeklyReturns:P4}
             """);
    }

    [Theory]
    [InlineData("sp500_daily_ohlc_30y_1996_2026.csv")]
    [InlineData("gbpusd_daily_ohlc_30y_1996_2026.csv")]
    [InlineData("soybeans_daily_ohlc_30y_1996_2026.csv")]
    public async Task GetReturns_GivenName_PlotLogReturn(string name)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Materials", name);
        var seriesName = Path.GetFileNameWithoutExtension(name);

        var series = await ReadPriceSeries(path);
        var returns = Enumerable.Skip<KeyValuePair<DateOnly, decimal>>(series, 1)
            .Zip(Enumerable.SkipLast<KeyValuePair<DateOnly, decimal>>(series, 1), (n, c) => (n.Key, Math.Log((double)n.Value / (double)c.Value)))
            .ToDictionary(p => p.Key, p => (decimal)Math.Round(p.Item2, 4));

        var total = 0.0m;
        var cumulativeReturn = returns.ToDictionary(p => p.Key, p =>
        {
            total += p.Value;
            return total;
        });

        var seriesPlot = PlotHelper.PlotSeries(seriesName, (IReadOnlyDictionary<DateOnly, decimal>)series, false);
        var returnsPlot = PlotHelper.PlotSeries($"returns of {seriesName}", returns, false);
        var cumulativeReturnsPlot = PlotHelper.PlotSeries($"cumulative returns of {seriesName}", cumulativeReturn, false);

        output.WriteLine($"{seriesName} => {seriesPlot.Path}");
        output.WriteLine($"returns of {seriesName} => {returnsPlot.Path}");
        output.WriteLine($"cumulative returns of {seriesName} => {cumulativeReturnsPlot.Path}");
    }

    /// <summary>
    /// reads a given file (by path) and parses time-series from it as dictionary - price by date
    /// </summary>
    /// <param name="path">full file path</param>
    /// <param name="column">1 - open, 2 - high, 3 - low, 4 - close prices</param>
    /// <param name="ct">cancellation token as the method is async</param>
    /// <returns>selected prices (close by default) per date</returns>
    private static async Task<Dictionary<DateOnly, decimal>> ReadPriceSeries(string path, int column = 4, CancellationToken? ct = null)
    {
        var series = new Dictionary<DateOnly, decimal>();
        
        await using var fileStream = new FileStream(path, new FileStreamOptions()
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            BufferSize = 1 << 16,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan
        });
        
        using var reader = new StreamReader(fileStream, Encoding.UTF8);
        await reader.ReadLineAsync(ct ?? CancellationToken.None);//header
        
        while (!reader.EndOfStream)
        {
            var row = await reader.ReadLineAsync(ct ?? CancellationToken.None);
            if (string.IsNullOrWhiteSpace(row))
                continue;

            var parts = row.Split(",");
            var date = DateOnly.ParseExact(parts[0], "yyyy-MM-dd");
            var price = decimal.Parse(parts[column]);
            series.Add(date, price);
        }

        return series;
    }
}