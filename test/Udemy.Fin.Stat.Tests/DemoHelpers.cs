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
}