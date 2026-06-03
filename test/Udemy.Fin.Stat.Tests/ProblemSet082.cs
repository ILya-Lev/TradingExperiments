using System.Numerics.Tensors;
using Parquet;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Integration")]
public class ProblemSet082(ITestOutputHelper output)
{
    [Theory]
    [InlineData("GSPC", "S&P 500")]
    [InlineData("RUT", "Russell2000")]
    [InlineData("SX5E", "STOXX500")]
    public async Task ClosePrices_SampleMean_Autocorrelation_Plot(string file, string indexName)
    {
        var closeSeries = await LoadData(file).ContinueWith(t => t.Result.ToArray());
        var logReturns = closeSeries.Skip(1).Zip(closeSeries.SkipLast(1), (n, p) => Math.Log(n / p) * 100).ToArray();

        var series = logReturns;
        var squareSeries = logReturns.Select(c => c * c).ToArray();
        var absSeries = logReturns.Select(Math.Abs).ToArray();
        
        var mean = TensorPrimitives.Sum(series) / series.Length;
        var variance = series.GetSampleVariance(mean);

        var acLogRet = series.GetAutoCorrelationParallel(series.Length - 1);
        var acSquareRet = squareSeries.GetAutoCorrelationParallel(squareSeries.Length - 1);
        var acAbs = absSeries.GetAutoCorrelationParallel(absSeries.Length - 1);

        output.WriteLine($"sample mean {mean}, variance {variance}, std dev {Math.Sqrt(variance)}");
        output.WriteLine($"auto correlation log ret 0 {acLogRet[0]}, 1 {acLogRet[1]}, 2 {acLogRet[2]}");
        output.WriteLine($"auto correlation square ret 0 {acSquareRet[0]}, 1 {acSquareRet[1]}, 2 {acSquareRet[2]}");
        output.WriteLine($"auto correlation abs close 0 {acAbs[0]}, 1 {acAbs[1]}, 2 {acAbs[2]}");

        var plotSeriesInfo = DemoHelpers.PlotSeries(indexName, "log returns", series, false);
        DemoHelpers.PlotSeries($"{indexName} log ret auto corr","auto correlation", acLogRet, false);
        DemoHelpers.PlotSeries($"{indexName} squared ret auto corr", "auto correlation", acSquareRet, false);
        DemoHelpers.PlotSeries($"{indexName} abs auto corr", "auto correlation", acAbs, false);
        output.WriteLine($"plotted {plotSeriesInfo.Path}");
    }

    /// <summary>
    /// before 1957-03-15 (inclusively) it was SnP90 as of 90 companies. After became SnP500 of 500 companies.
    /// </summary>
    [Theory]
    [InlineData("", "1957-03-15")]
    [InlineData("1957-03-16", "")]
    [InlineData("1991-07-01", "1996-12-31")]
    public async Task ClosePrices_Autocorrelation_SnpSubset_Plot(string startDate, string endDate)
    {
        var fromDate = string.IsNullOrWhiteSpace(startDate) ? DateOnly.MinValue : DateOnly.Parse(startDate);
        var toDate = string.IsNullOrWhiteSpace(endDate) ? DateOnly.MaxValue : DateOnly.Parse(endDate);
        var dateFilter = (DateOnly d) => fromDate <= d && d <= toDate;

        var indexName = "S&P500";
        var file = "GSPC";
        
        var closeSeries = await LoadData(file, dateFilter).ContinueWith(t => t.Result.ToArray());
        
        //var logReturns = closeSeries.Skip(1).Zip(closeSeries.SkipLast(1), (n, p) => Math.Log(n / p) * 100).ToArray();
        var returns = closeSeries.Skip(1).Zip(closeSeries.SkipLast(1), (n, p) => n-p).ToArray();
        var absReturns = returns.Select(Math.Abs).ToArray();
        var acAbs = absReturns.GetAutoCorrelationParallel(absReturns.Length - 1);

        DemoHelpers.PlotSeries($"{indexName} returns {startDate}--{endDate}","returns", returns, false);
        DemoHelpers.PlotSeries($"{indexName} abs ret auto corr {startDate}--{endDate}","auto correlation", acAbs, false);
    }

    private static async Task<IEnumerable<double>> LoadData(string file, Func<DateOnly, bool>? dateFilter = null)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", $"{file}.parquet");
        try
        {
            return await DataLoader.LoadParquet<ExIndex>(path)
                .ContinueWith(t => t.Result
                    .Where(p => dateFilter?.Invoke(p.Date) ?? true)
                    .Select(p => (double)p.Close));
        }
        catch (Exception exc)
        {
            //motivation: file contains either ExIndex or ExOhlc => if the first fails, try with the last one.
            return await DataLoader.LoadParquet<ExOhlc>(path)
                .ContinueWith(t => t.Result
                    .Where(p => dateFilter?.Invoke(p.Date) ?? true)
                    .Select(p => (double)p.Close));
        }
    }

    private static IEnumerable<double> GetLogarithmicReturns(IEnumerable<double> prices)
    {
        using var iterator = prices.GetEnumerator();
        if (!iterator.MoveNext())
            yield break;

        var previous = iterator.Current;
        while (iterator.MoveNext())
        {
            var ratio = iterator.Current / previous;
            if (ratio < 0 || ratio == 0 || !double.IsFinite(ratio))
                continue;

            yield return 100 * Math.Log(ratio);
            previous = iterator.Current;
        }
    }
}