using ScottPlot;

namespace Udemy.Fin.Stat.Tests;

public class DemoHelpers
{
    public static SavedImageInfo PlotSeries(string sourceName, string dataNature, double[] series, bool isHistogram = false)
    {
        var chartName = $"{sourceName} {series.Length} {dataNature}.svg";
        var chartPath = Path.Combine(Directory.GetCurrentDirectory(), "charts", chartName);
        Directory.CreateDirectory(Path.GetDirectoryName(chartPath) ?? throw new InvalidOperationException());

        var plot = new Plot();
        plot.Title(sourceName);
        var palette = new ScottPlot.Palettes.Category10();


        if (isHistogram)
        {
            // 1. Calculate optimal bin count using Sturges' Rule
            //int binCount = (int)Math.Ceiling(1 + Math.Log2(series.Length));
            int binCount = 1000;

            // 2. Generate the histogram with the calculated count
            var hist = ScottPlot.Statistics.Histogram.WithBinCount(binCount, series);

            // 3. Plot it
            plot.Add.Histogram(hist);
        }
        else
        {
            var line = plot.Add.ScatterLine
            (
                Enumerable.Range(0, series.Length).ToArray(),
                series
            );
            line.Color = palette.GetColor(0);
            line.LineWidth = 2;
        }


        var zeroLevel = plot.Add.ScatterLine
        (
            Enumerable.Range(0, series.Length).ToArray(),
            Enumerable.Repeat(0, series.Length).ToArray()
        );
        zeroLevel.Color = palette.GetColor(4);
        zeroLevel.LineWidth = 2;

        plot.Axes.Right.IsVisible = true;
        plot.Axes.Top.IsVisible = true;

        plot.RenderManager.RenderStarting += (sender, args) =>
        {
            plot.Axes.Right.Min = plot.Axes.Left.Min;
            plot.Axes.Right.Max = plot.Axes.Left.Max;
            plot.Axes.Top.Min = plot.Axes.Bottom.Min;
            plot.Axes.Top.Max = plot.Axes.Bottom.Max;
        };

        return plot.SaveSvg(chartPath, 1980, 1020);
    }

    public static async Task<IEnumerable<double>> LoadClosePrices(string file, Func<DateOnly, bool>? dateFilter = null)
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

    public static SavedImageInfo PlotSeries(string sourceName
        , IReadOnlyCollection<(double x, double y)> series
        , bool addDiagonal = true)
    {
        var chartName = $"{sourceName}.svg";
        var chartPath = Path.Combine(Directory.GetCurrentDirectory(), "charts", chartName);
        Directory.CreateDirectory(Path.GetDirectoryName(chartPath) ?? throw new InvalidOperationException());

        var plot = new Plot();
        plot.Title(sourceName);
        var palette = new ScottPlot.Palettes.Category10();

        var line = plot.Add.Scatter
        (
            series.Select(p => new Coordinates(p.x, p.y)).ToArray()
        );
        line.Color = palette.GetColor(0);
        line.LineWidth = 2;

        if (addDiagonal)
        {
            var from = Math.Min(series.First().x, series.First().y);
            var to = Math.Max(series.Last().x, series.Last().y);
            var diagonalPoints = new List<Coordinates>();
            for (double x = from; x <= to; x += 0.01)
            {
                diagonalPoints.Add(new Coordinates(x, x));
            }
            var mainDiagonal = plot.Add.ScatterLine(diagonalPoints);
            mainDiagonal.Color = palette.GetColor(4);
            mainDiagonal.LineWidth = 2;
        }

        plot.Axes.Right.IsVisible = true;
        plot.Axes.Top.IsVisible = true;

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