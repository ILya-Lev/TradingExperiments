using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using ScottPlot;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Integration")]
public class ProblemSet072(ITestOutputHelper output)
{
    [Fact]
    public void WhiteNoise_Generate_Plot()
    {
        var wn = AlglibWhiteNoise.Generate(1.0).Take(1000).ToArray();
        PlotSeries("white noise", "gaussian", wn);
        output.WriteLine($"generated {wn.Length} points of the white noise");
    }

    [Fact]
    public void Autocorrelation_ForWhiteNoise_Generate_Plot()
    {
        var wn = AlglibWhiteNoise.Generate(1.0).Take(1000).ToArray();
        var acfs = wn.GetAutoCorrelationSimd(100);
        var acf = wn.GetAutoCorrelation(100);
        var acfp = wn.GetAutoCorrelationParallel(100);
        PlotSeries("auto correlation", "gaussian white noise", acfs);
        output.WriteLine($"generated {acfs.Length} points of the white noise auto correlation");

        //assertions reflect that the correctly calculated acf has the first point = 1, the lat = 0
        acfs[0].Should().BeApproximately(1.0, 1e-6);
        acfs[99].Should().BeApproximately(0.0, 7e-2);
        acfs[100].Should().BeApproximately(0.0, 5e-2);

        for (int i = 0; i < acfs.Length; i++)
        {
            acfs[i].Should().BeApproximately(acf[i], 3e-2);
            acfp[i].Should().BeApproximately(acf[i], 3e-2);
        }
    }

    private static SavedImageInfo PlotSeries(string sourceName, string dataNature, double[] series)
    {
        var chartName = $"{sourceName} {series.Length} {dataNature}.svg";
        var chartPath = Path.Combine(Directory.GetCurrentDirectory(), "charts", chartName);
        Directory.CreateDirectory(Path.GetDirectoryName(chartPath) ?? throw new InvalidOperationException());

        var plot = new Plot();
        plot.Title(sourceName);
        var palette = new ScottPlot.Palettes.Category10();

        var line = plot.Add.ScatterLine
        (
            Enumerable.Range(0, series.Length).ToArray(),
            series
        );
        line.Color = palette.GetColor(0);
        line.LineWidth = 2;

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