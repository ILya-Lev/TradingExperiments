using MathNet.Numerics.LinearAlgebra.Factorization;
using ScottPlot;
using System.Globalization;

namespace Udemy.Fin.Stat.Tests;

using SeriesType = IReadOnlyDictionary<DateOnly, decimal>;

[Trait("Category", "Integration")]
public class ProblemSet071(ITestOutputHelper output)
{
    [Theory]
    [InlineData("gspc", null, null)]
    [InlineData("gspc", "20010101", "20101231")]
    [InlineData("rut", null, null)]
    [InlineData("rut", "20010101", "20101231")]
    [InlineData("sx5e", null, null)]
    [InlineData("sx5e", "20010101", "20101231")]
    public async Task Load_ClosingPrices_Plot(string sourceName, string? start, string? end)
    {
        var closePrices = await GetClosePrices(sourceName, start, end);
        var savedImageInfo = PlotSeries(sourceName, "closePrices", closePrices);

        output.WriteLine($"loaded {closePrices.Count} {sourceName} close prices from {closePrices.First().Key} to {closePrices.Last().Key}");
        output.WriteLine(savedImageInfo.ToString());
    }

    [Theory]
    [InlineData("gspc", null, null)]
    [InlineData("gspc", "20010101", "20101231")]
    [InlineData("rut", null, null)]
    [InlineData("rut", "20010101", "20101231")]
    [InlineData("sx5e", null, null)]
    [InlineData("sx5e", "20010101", "20101231")]
    public async Task Load_DailyLogarithmicReturns_Plot(string sourceName, string? start, string? end)
    {
        var closePrices = await GetClosePrices(sourceName, start, end);

        var logarithmicReturns = GetLogarithmicReturns(closePrices.ToDictionary(p => p.Key, p => (double)p.Value))
            .ToDictionary(p => p.Item1, p => (decimal)Math.Round(p.Item2, 4));

        var savedImageInfo = PlotSeries(sourceName, "logarithmic returns", logarithmicReturns);
        output.WriteLine(savedImageInfo.ToString());
    }

    [Theory]
    [InlineData("gspc", null, null)]
    [InlineData("gspc", "20010101", "20101231")]
    [InlineData("rut", null, null)]
    [InlineData("rut", "20010101", "20101231")]
    [InlineData("sx5e", null, null)]
    [InlineData("sx5e", "20010101", "20101231")]
    public async Task Load_NetReturns_Plot(string sourceName, string? start, string? end)
    {
        var closePrices = await GetClosePrices(sourceName, start, end);

        var logarithmicReturns = GetNetReturns(closePrices.ToDictionary(p => p.Key, p => (double)p.Value))
            .ToDictionary(p => p.Item1, p => (decimal)Math.Round(p.Item2, 4));

        var savedImageInfo = PlotSeries(sourceName, "net returns", logarithmicReturns);
        output.WriteLine(savedImageInfo.ToString());
    }

    [Theory]
    [InlineData( null, null)]
    [InlineData("20010101", "20101231")]
    public async Task Load_DailyLogarithmicReturns_CompareIndices_Plot(string? start, string? end)
    {
        var names = new[] { "gspc", "rut", "sx5e" };
        var curves = new Dictionary<string, SeriesType>();
        foreach (var name in names)
        {
            var closePrices = await GetClosePrices(name, start, end);
            var logarithmicReturns = GetLogarithmicReturns(closePrices.ToDictionary(p => p.Key, p => (double)p.Value))
                .ToDictionary(p => p.Item1, p => (decimal)Math.Round(p.Item2, 4));
            curves.Add(name, logarithmicReturns);
        }
        
        var savedImageInfo = PlotSeries("comparison", "logarithmic returns", curves);
        output.WriteLine(savedImageInfo.ToString());
    }

    [Theory]
    [InlineData( null, null)]
    [InlineData("20010101", "20101231")]
    public async Task Load_DailyNetReturns_CompareIndices_Plot(string? start, string? end)
    {
        var names = new[] { "gspc", "rut", "sx5e" };
        var curves = new Dictionary<string, SeriesType>();
        foreach (var name in names)
        {
            var closePrices = await GetClosePrices(name, start, end);
            var logarithmicReturns = GetNetReturns(closePrices.ToDictionary(p => p.Key, p => (double)p.Value))
                .ToDictionary(p => p.Item1, p => (decimal)Math.Round(p.Item2, 4));
            curves.Add(name, logarithmicReturns);
        }
        
        var savedImageInfo = PlotSeries("comparison", "net returns", curves);
        output.WriteLine(savedImageInfo.ToString());
    }

    [Fact]
    public async Task Load_LogarithmicReturns_CompareFullVsTruncated_Plot()
    {
        var startDate = DateOnly.ParseExact("20000101","yyyyMMdd", CultureInfo.InvariantCulture);
        var endDate = DateOnly.ParseExact("20200101", "yyyyMMdd", CultureInfo.InvariantCulture);

        var curves = new Dictionary<string, SeriesType>();
        
        var closePricesFull = await GetClosePrices("gspc", null, null);
        var logarithmicReturnsFull = GetLogarithmicReturns(closePricesFull.ToDictionary(p => p.Key, p => (double)p.Value))
            .Where(p => startDate <= p.Item1 && p.Item1 <= endDate)
            .ToDictionary(p => p.Item1, p => (decimal)Math.Round(p.Item2, 4));
        curves.Add("gspc full", logarithmicReturnsFull);

        var closePricesTrimmed = await GetClosePrices("gspc", "20000101", "20200101");
        var logarithmicReturnsTrimmed = GetLogarithmicReturns(closePricesTrimmed.ToDictionary(p => p.Key, p => (double)p.Value))
            .ToDictionary(p => p.Item1, p => (decimal)Math.Round(p.Item2, 4));
        curves.Add("gspc trimmed", logarithmicReturnsTrimmed);
        
        var savedImageInfo = PlotSeries("full vs trimmed", "log returns", curves);
        output.WriteLine(savedImageInfo.ToString());
    }

    private static IEnumerable<(DateOnly, double)> GetLogarithmicReturns(IReadOnlyDictionary<DateOnly, double> prices)
    {
        using var iterator = prices.GetEnumerator();
        if (!iterator.MoveNext())
            yield break;
        var previous = iterator.Current;
        while (iterator.MoveNext())
        {
            var ratio = iterator.Current.Value / previous.Value;
            if (ratio < 0 || ratio == 0 || !double.IsFinite(ratio))
                continue;

            yield return (iterator.Current.Key, 100 * Math.Log(ratio));
        }
    }

    private static IEnumerable<(DateOnly, double)> GetNetReturns(IReadOnlyDictionary<DateOnly, double> prices)
    {
        using var iterator = prices.GetEnumerator();
        if (!iterator.MoveNext())
            yield break;
        var previous = iterator.Current;
        while (iterator.MoveNext())
        {
            var netReturn = (iterator.Current.Value - previous.Value)/ previous.Value;
            if (!double.IsFinite(netReturn))
                continue;

            yield return (iterator.Current.Key, 100 * netReturn);
        }
    }

    private static async Task<SeriesType> GetClosePrices(string sourceName, string? start, string? end)
    {
        var startDate = DateOnly.ParseExact(start ?? "17000101", "yyyyMMdd", CultureInfo.InvariantCulture);
        var endDate = DateOnly.ParseExact(end ?? "22000101", "yyyyMMdd", CultureInfo.InvariantCulture);
        
        var closePrices = await DoGetClosePrices(sourceName);
        
        return closePrices.Where(p => startDate <= p.Key && p.Key <= endDate).ToDictionary();
    }

    private static async Task<SeriesType> DoGetClosePrices(string sourceName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", $"{sourceName}.parquet");
        try
        {
            return await DataLoader.LoadParquet<ExIndex>(path)
                .ContinueWith(t => t.Result.ToDictionary(p => p.Date, p => p.Close));
        }
        catch (Exception exc)
        {
            //motivation: file contains either ExIndex or ExOhlc => if the first fails, try with the last one.
            return await DataLoader.LoadParquet<ExOhlc>(path)
                .ContinueWith(t => t.Result.ToDictionary(p => p.Date, p => p.Close));
        }
    }

    private static SavedImageInfo PlotSeries(string sourceName, string dataNature, SeriesType closePrice)
        => PlotSeries(sourceName, dataNature, new Dictionary<string, SeriesType>()
        {
            [sourceName] = closePrice
        });

    private static SavedImageInfo PlotSeries(string sourceName, string dataNature, Dictionary<string, SeriesType> series)
    {
        var chartName = $"{sourceName} {series.Sum(p => p.Value.Count)} {dataNature}.svg";
        var chartPath = Path.Combine(Directory.GetCurrentDirectory(), "charts", chartName);
        Directory.CreateDirectory(Path.GetDirectoryName(chartPath) ?? throw new InvalidOperationException());
        
        var plot = new Plot();
        var palette = new ScottPlot.Palettes.Category10();

        for (var i = 0; i < series.Count; i++)
        {
            var closePrice = series.ToArray()[i];
            var line = plot.Add.ScatterLine
            (
                closePrice.Value.Keys.Select(d => d.ToDateTime(TimeOnly.MinValue)).ToArray(),
                closePrice.Value.Values.ToArray()
            );
            line.Color = palette.GetColor(i);
            line.LineWidth = 2;
            line.LegendText = series.ToArray()[i].Key;
        }
        
        plot.Axes.Right.IsVisible = true;
        plot.Axes.Top.IsVisible = true;

        plot.Axes.DateTimeTicksBottom();
        plot.Axes.Top.TickGenerator = new ScottPlot.TickGenerators.DateTimeAutomatic();
        
        plot.RenderManager.RenderStarting += (sender, args) =>
        {
            plot.Axes.Right.Min = plot.Axes.Left.Min;
            plot.Axes.Right.Max = plot.Axes.Left.Max;
            plot.Axes.Top.Min = plot.Axes.Bottom.Min;
            plot.Axes.Top.Max = plot.Axes.Bottom.Max;
        };

        return plot.SaveSvg(chartPath, 1980, 1020);
    }

}