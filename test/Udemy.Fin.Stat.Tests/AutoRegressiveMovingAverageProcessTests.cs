using FluentAssertions;

namespace Udemy.Fin.Stat.Tests;

public class AutoRegressiveMovingAverageProcessTests(ITestOutputHelper output)
{
    [Fact]
    public void GenerateArma_Zero8Zero1_Plot()
    {
        double[] phis = [.8, .1];
        AutoRegressiveMovingAverageProcess.IsStationary(phis[0], phis[1]).Should().BeTrue();
        var series = AutoRegressiveMovingAverageProcess.GenerateArma(phis, []).Take(1_000).ToArray();
        var imageDescription = DemoHelpers.PlotSeries("ar 2", "0.8 and 0.1", series);
        output.WriteLine($"have plotted {imageDescription.Path} of {series.Length} points");
    }

    [Fact]
    public void GenerateArma_Zero5Zero3_Plot()
    {
        double[] phis = [.5, .3];
        AutoRegressiveMovingAverageProcess.IsStationary(phis[0], phis[1]).Should().BeTrue();
        var series = AutoRegressiveMovingAverageProcess.GenerateArma(phis, []).Take(1_000).ToArray();
        var imageDescription = DemoHelpers.PlotSeries("ar 2", "0.5 and 0.3", series);
        output.WriteLine($"have plotted {imageDescription.Path} of {series.Length} points");
    }

    [Fact]
    public void GetAcf_Zero5Zero3_Plot()
    {
        double[] phis = [.5, .3];
        AutoRegressiveMovingAverageProcess.IsStationary(phis[0], phis[1]).Should().BeTrue();
        var series = AutoRegressiveMovingAverageProcess.GetAutoCorrelationFunction(phis[0], phis[1]).Take(1_000).ToArray();
        var imageDescription = DemoHelpers.PlotSeries
        (
            "ar 2 acf",
            "0.5 and 0.3",
            series,
            isHistogram: true
        );
        output.WriteLine($"have plotted {imageDescription.Path} of {series.Length} points");
    }

    [Theory]
    [InlineData(1_000)]
    [InlineData(10_000)]
    [InlineData(100_000)]
    public void GenerateWhiteNoise_OfGivenSize_Plot(int length)
    {
        var series = AutoRegressiveMovingAverageProcess.GenerateWhiteNoise().Take(length).ToArray();
        var imageDescription = DemoHelpers.PlotSeries("white noise", length.ToString(), series);
        output.WriteLine($"have plotted {imageDescription.Path} of {series.Length} points");
    }
}

