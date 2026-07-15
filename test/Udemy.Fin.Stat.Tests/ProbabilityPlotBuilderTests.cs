using MathNet.Numerics.Distributions;
using ScottPlot;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class ProbabilityPlotBuilderTests(ITestOutputHelper output)
{
    /// <summary>
    /// we observed some data (sample)
    /// we assume it is distributed as Student's - T with mean = 0, std dev = 1, and 2 degrees of freedom
    /// the plot will show us where are we with our assumption
    /// if we are right, the plot converges to y=x - main diagonal (the Glivenko-Cantelli theorem)
    /// </summary>
    [Fact]
    public void GetPoints_Sample001_Observe()
    {
        var sample = new[] { -.52, -.33, 2.55, -5.5, 3.7, -.4, -.65, -.03, .22, -1.35, 1.62, -3 };

        var invCdf = (double p) => StudentT.InvCDF(0.0, 1.0, 2, p);

        var points = sample.GetPoints(invCdf).ToArray();

        foreach (var (x, y) in points) output.WriteLine($"({x}, {y})");

        var info = PlotSeries("student2 probability plot", points);
        output.WriteLine(info.Path);
    }

    [Fact]
    public void GetPoints_Normal3s9_VsNormal()
    {
        var sample = Normal.Samples(3, 3).Take(1_000).ToArray();

        var invCdf = (double p) => Normal.InvCDF(0.0, 1.0, p);

        var points = sample.GetPoints(invCdf).ToArray();

        //foreach (var (x, y) in points) output.WriteLine($"({x}, {y})");

        var info = PlotSeries("normal mu 3 sigma 3 probability plot", points);
        output.WriteLine(info.Path);
    }

    public static SavedImageInfo PlotSeries(string sourceName, IReadOnlyCollection<(double x, double y)> series)
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

        var from = Math.Min(series.First().x, series.First().y);
        var to = Math.Max(series.Last().x, series.Last().y);
        var diagonalPoints = new List<Coordinates>();
        for (double x = from; x <= to; x+= 0.01)
        {
            diagonalPoints.Add(new Coordinates(x,x));
        }
        var mainDiagonal = plot.Add.ScatterLine(diagonalPoints);
        mainDiagonal.Color = palette.GetColor(4);
        mainDiagonal.LineWidth = 2;

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