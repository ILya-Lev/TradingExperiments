using System.Globalization;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Extensions;
using MathNet.Numerics.Statistics;

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

    [Fact]
    public void GetCumulativeReturn_ArithmeticSequence_CorrectSlidingWindow()
    {
        var series = Enumerable.Range(1, 100).ToArray();

        const int windowSize = 10;
        var sumWindow10 = series.GetCumulativeReturn(windowSize).ToArray();

        using var _ = new AssertionScope();
        var total = 55; //as the sum of the first 10 numbers is 55
        for (var i = windowSize; i < series.Length; i++)
        {
            sumWindow10[i - windowSize].Should().Be(total);
            total += series[i];
            total -= series[i - windowSize];
        }

        //another way to assert things
        for (int i = 1; i < sumWindow10.Length; i++)
        {
            sumWindow10[i].Should().Be(sumWindow10[i - 1] + 10);
        }
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
        var returns = series.Skip(1)
            .Zip(series.SkipLast(1), (n, c) => (n.Key, Math.Log((double)n.Value / (double)c.Value)))
            .ToDictionary(p => p.Key, p => (decimal)Math.Round(p.Item2, 4));

        var total = 0.0m;
        var cumulativeReturn = returns.ToDictionary(p => p.Key, p =>
        {
            total += p.Value;
            return total;
        });

        var seriesPlot = PlotHelper.PlotSeries(seriesName, series, false);
        var returnsPlot = PlotHelper.PlotSeries($"returns of {seriesName}", returns, false);
        var cumulativeReturnsPlot = PlotHelper.PlotSeries($"cumulative returns of {seriesName}", cumulativeReturn, false);

        output.WriteLine($"{seriesName} => {seriesPlot.Path}");
        output.WriteLine($"returns of {seriesName} => {returnsPlot.Path}");
        output.WriteLine($"cumulative returns of {seriesName} => {cumulativeReturnsPlot.Path}");
    }
    
    [Theory]
    [InlineData("sp500_daily_ohlc_30y_1996_2026.csv")]
    [InlineData("gbpusd_daily_ohlc_30y_1996_2026.csv")]
    [InlineData("soybeans_daily_ohlc_30y_1996_2026.csv")]
    public async Task GetReturns_GivenName_PlotAutocorrelations(string name)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Materials", name);
        var seriesName = Path.GetFileNameWithoutExtension(name);

        var series = await ReadPriceSeries(path);
        var returns = series.Skip(1)
            .Zip(series.SkipLast(1), (n, c) => (n.Key, Math.Log((double)n.Value / (double)c.Value)))
            .ToDictionary(p => p.Key, p => (decimal)Math.Round(p.Item2, 4));

        var rawAutocorrelations = returns.Select(p => (double)p.Value).ToArray().GetAutocorrelationPerLag(30);
        var absAutocorrelations = returns.Select(p => (double)Math.Abs(p.Value)).ToArray().GetAutocorrelationPerLag(30);

        var rawToPlot = rawAutocorrelations.Select((a, i) => ((decimal)i, (decimal)a)).ToArray();
        var absToPlot = absAutocorrelations.Select((a, i) => ((decimal)i, (decimal)a)).ToArray();

        var rawPlot = PlotHelper.PlotSeries($"raw autocorr of {seriesName}", rawToPlot, false);
        var absPlot = PlotHelper.PlotSeries($"abs autocorr of {seriesName}", absToPlot, false);

        var returnsKurtosis = returns.Select(p => (double)p.Value).Kurtosis();
        output.WriteLine($"returns excessive kurtosis is {returnsKurtosis}");
        output.WriteLine($"raw autocorrelations for {seriesName} => {rawPlot.Path}");
        output.WriteLine($"abs autocorrelations for {seriesName} => {absPlot.Path}");
    }

    [Theory]
    [InlineData("sp500_daily_ohlc_30y_1996_2026.csv")]
    [InlineData("gbpusd_daily_ohlc_30y_1996_2026.csv")]
    [InlineData("soybeans_daily_ohlc_30y_1996_2026.csv")]
    public async Task GetMonthlyReturns_GivenName_PlotAutocorrelations(string name)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Materials", name);
        var seriesName = Path.GetFileNameWithoutExtension(name);

        var series = await ReadPriceSeries(path);
        var returns = series.Skip(1)
            .Zip(series.SkipLast(1), (n, c) => (n.Key, Math.Log((double)n.Value / (double)c.Value)))
            .ToDictionary(p => p.Key, p => (decimal)Math.Round(p.Item2, 4));

        //here we do aggregation to have monthly instead of daily return - 20 working days in a month
        var rawAutocorrelations = returns.Select(p => (double)p.Value).GetCumulativeReturn(20).ToArray().GetAutocorrelationPerLag(70);
        var absAutocorrelations = returns.Select(p => (double)Math.Abs(p.Value)).GetCumulativeReturn(20).ToArray().GetAutocorrelationPerLag(70);

        var rawToPlot = rawAutocorrelations.Select((a, i) => ((decimal)i, (decimal)a)).ToArray();
        var absToPlot = absAutocorrelations.Select((a, i) => ((decimal)i, (decimal)a)).ToArray();

        var rawPlot = PlotHelper.PlotSeries($"monthly raw autocorr of {seriesName}", rawToPlot, false);
        var absPlot = PlotHelper.PlotSeries($"monthly abs autocorr of {seriesName}", absToPlot, false);

        var returnsKurtosis = returns.Select(p => (double)p.Value).Kurtosis();
        output.WriteLine($"returns excessive kurtosis is {returnsKurtosis}");
        output.WriteLine($"raw autocorrelations for {seriesName} => {rawPlot.Path}");
        output.WriteLine($"abs autocorrelations for {seriesName} => {absPlot.Path}");
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
        await using var fileStream = new FileStream(path, new FileStreamOptions()
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            BufferSize = 1 << 16,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan
        });
        
        using var reader = new StreamReader(fileStream, Encoding.UTF8);
        await reader.ReadLineAsync(ct ?? CancellationToken.None);//header

        var estimatedRows = (int)(fileStream.Length / 38);
        var series = new Dictionary<DateOnly, decimal>(estimatedRows);

        while (await reader.ReadLineAsync(ct ?? CancellationToken.None) is {} row)
        {
            if (string.IsNullOrWhiteSpace(row)) continue;       //skip empty lines
            var rowSpan = row.AsSpan();
            
            var rowIterator = rowSpan.Split(',');
            if (!rowIterator.MoveNext()) continue;              //skip lines without comas
            var date = DateOnly.ParseExact(rowSpan[rowIterator.Current], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            
            var targetPart = ReadTargetColumn(column, ref rowIterator, rowSpan);
            if (targetPart.IsEmpty) continue;                   //skip the line if we cannot find target column
            var price = decimal.Parse(targetPart, CultureInfo.InvariantCulture);
            
            series.Add(date, price);
        }

        return series;
    }

    private static ReadOnlySpan<char> ReadTargetColumn(
        int targetColumn
        , ref MemoryExtensions.SpanSplitEnumerator<char> rowIterator
        , ReadOnlySpan<char> rowSpan)
    {
        for(var currentColumn = 0; currentColumn < targetColumn; currentColumn++)
        {
            if (!rowIterator.MoveNext())
                return ReadOnlySpan<char>.Empty;
        }

        return rowSpan[rowIterator.Current];
    }
}