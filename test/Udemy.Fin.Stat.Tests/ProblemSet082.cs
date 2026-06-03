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
    [InlineData("GSPC", "S&P 500")]
    public async Task ClosePrices_Autocorrelation_SnP90_vs_SnP500_Plot(string file, string indexName)
    {
        var thresholdDate = DateOnly.Parse("1957-03-15");
        
        var closeSeries90 = await LoadData(file, d => d <= thresholdDate).ContinueWith(t => t.Result.ToArray());
        var closeSeries500 = await LoadData(file, d => d > thresholdDate).ContinueWith(t => t.Result.ToArray());
        
        //var logReturns = closeSeries.Skip(1).Zip(closeSeries.SkipLast(1), (n, p) => Math.Log(n / p) * 100).ToArray();
        var returns90 = closeSeries90.Skip(1).Zip(closeSeries90.SkipLast(1), (n, p) => n-p).ToArray();
        var absReturns90 = returns90.Select(Math.Abs).ToArray();

        var returns500 = closeSeries500.Skip(1).Zip(closeSeries500.SkipLast(1), (n, p) => n-p).ToArray();
        var absReturns500 = returns500.Select(Math.Abs).ToArray();
        
        var acAbs90 = absReturns90.GetAutoCorrelationParallel(absReturns90.Length - 1);
        var acAbs500 = absReturns500.GetAutoCorrelationParallel(absReturns500.Length - 1);

        DemoHelpers.PlotSeries($"{indexName} abs ret 90 auto corr","auto correlation", acAbs90, false);
        DemoHelpers.PlotSeries($"{indexName} abs ret 500 auto corr","auto correlation", acAbs500, false);
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