using MathNet.Numerics.Distributions;

namespace Udemy.Derivatives.Tests;

[Trait("Category", "Unit")]
public class QqPlotTests(ITestOutputHelper output)
{
    [Fact]
    public void QqPlot_Sample2kNormal_Observe()
    {
        // observed quantiles = the sorted sample values (order statistics)
        var observed = Normal.Samples(0, 1).Take(2_000).Order().ToArray();

        // theoretical quantiles = Inverse CDF of the rank probability p_i (Blom)
        var denominator = observed.Length + .25;
        var theoretical = observed
            .Select((_, i) => Normal.InvCDF(0, 1, (i+.625)/denominator))
            .ToArray();

        var points = theoretical.Zip(observed, (t, b) => ((decimal)t, (decimal)b)).ToArray();
        var plot = PlotHelper.PlotSeries("q-q plot normal", points, true);
        output.WriteLine($"q-q plot for normal distribution {plot.Path}");
    }

    [Fact]
    public void QqPlot_Sample2kStudent_Observe()
    {
        var freedom = 4;

        // observed quantiles = the sorted sample values (order statistics)
        var observed = StudentT.Samples(0, 1, freedom).Take(2_000).Order().ToArray();

        // theoretical quantiles = Inverse CDF of the rank probability p_i (Hazen midpoint: (i+.5)/n)
        var theoretical = observed
            .Select((_, i) => StudentT.InvCDF(0, 1, freedom, (i+.5)/observed.Length))
            .ToArray();

        var points = theoretical.Zip(observed, (t, b) => ((decimal)t, (decimal)b)).ToArray();

        var plot = PlotHelper.PlotSeries("q-q plot Student-T", points, true);
        output.WriteLine($"q-q plot for Student-T distribution {plot.Path}");
    }
}