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

        var info = DemoHelpers.PlotSeries("student2 probability plot", points);
        output.WriteLine(info.Path);
    }

    [Fact]
    public void GetPoints_Normal3s9_VsNormal()
    {
        var sample = Normal.Samples(3, 3).Take(1_000).ToArray();

        var invCdf = (double p) => Normal.InvCDF(0.0, 1.0, p);

        var points = sample.GetPoints(invCdf).ToArray();

        //foreach (var (x, y) in points) output.WriteLine($"({x}, {y})");

        var info = DemoHelpers.PlotSeries("normal mu 3 sigma 3 probability plot", points);
        output.WriteLine(info.Path);
    }

    [Theory]
    [InlineData("20030701", "20061231")]
    [InlineData("19920101", "19961231")]
    [InlineData("19830101", "19860731")]
    public async Task GetPoints_SnP500_VsNormal(string startStr, string endStr)
    {
        var startDate = DateOnly.ParseExact(startStr, "yyyyMMdd");
        var endDate = DateOnly.ParseExact(endStr, "yyyyMMdd");
        var filter = (DateOnly d) => startDate <= d && d <= endDate;

        var sample = await DemoHelpers.LoadClosePrices("GSPC", filter).ContinueWith(t => t.Result.ToArray());

        var invCdf = (double p) => Normal.InvCDF(0.0, 1.0, p);

        var points = sample.GetPoints(invCdf).Select(p => (p.y, p.x)).ToArray();

        //foreach (var (x, y) in points) output.WriteLine($"({x}, {y})");

        var info = DemoHelpers.PlotSeries($"S&P500 {startStr} - {endStr} probability plot", points, false);
        output.WriteLine(info.Path);
    }
}