using MathNet.Numerics.Distributions;
using ScottPlot;

namespace Udemy.Derivatives.Tests;

[Trait("Category", "Unit")]
public class AutocorrelationCalculatorTests(ITestOutputHelper output)
{
    [Fact]
    public void GetAutocorrelationPerLag_Sample2k_RawAndTransformed()
    {
        var sample = Normal.Samples(0, 1).Take(2_000).ToArray();

        var lagged = Enumerable.Range(2, sample.Length - 2)
            .Select(t => 0.5 * sample[t - 2] + 0.3 * sample[t - 1] + 0.2 * sample[t])
            .ToArray();

        var originalAutocorrelation = sample.GetAutocorrelationPerLag();
        var laggedAutocorrelation = lagged.GetAutocorrelationPerLag();

        var originalImage = PlotSeries("original", originalAutocorrelation.Select((a,i) => ((decimal) i, (decimal) a)).ToArray(), false);
        var laggedImage = PlotSeries("lagged", laggedAutocorrelation.Select((a,i) => ((decimal) i, (decimal) a)).ToArray(), false);

        output.WriteLine($"original sample autocorrelation: {originalImage.Path}");
        output.WriteLine($"lagged sample autocorrelation: {laggedImage.Path}");
    }

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

}
