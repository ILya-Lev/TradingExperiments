using ScottPlot;

namespace Udemy.Derivatives.Tests;

public static class PlotHelper
{
    public static SavedImageInfo PlotSeries(string sourceName
        , IReadOnlyCollection<(decimal x, decimal y)> series
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
            series.Select(p => new Coordinates((double)p.x, (double)p.y)).ToArray()
        );
        line.Color = palette.GetColor(0);
        line.LineWidth = 2;

        if (addDiagonal)
        {
            var from = Math.Min(series.First().x, series.First().y);
            var to = Math.Max(series.Last().x, series.Last().y);
            var diagonalPoints = new List<Coordinates>();
            for (var x = from; x <= to; x += 0.01m)
            {
                diagonalPoints.Add(new Coordinates((double)x, (double)x));
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

    public static SavedImageInfo PlotSeries(string sourceName
        , IReadOnlyDictionary<DateOnly, decimal> series
        , bool addDiagonal = true)
    {
        var chartName = $"{sourceName}.svg";
        var chartPath = Path.Combine(Directory.GetCurrentDirectory(), "charts", chartName);
        Directory.CreateDirectory(Path.GetDirectoryName(chartPath) ?? throw new InvalidOperationException());

        var plot = new Plot();
        plot.Title(sourceName);
        var palette = new ScottPlot.Palettes.Category10();

        var ordered = series.OrderBy(p => p.Key).ToArray();
        var dates = ordered.Select(p => p.Key.ToDateTime(TimeOnly.MinValue)).ToArray();
        var values = ordered.Select(p => (double)p.Value).ToArray();

        var line = plot.Add.ScatterLine(dates, values);
        line.Color = palette.GetColor(0);
        line.LineWidth = 2;

        if (addDiagonal && ordered.Length >= 2)
        {
            var mainDiagonal = plot.Add.ScatterLine(
                new[] { dates[0], dates[^1] },
                new[] { values[0], values[^1] }
            );
            mainDiagonal.Color = palette.GetColor(4);
            mainDiagonal.LineWidth = 2;
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
