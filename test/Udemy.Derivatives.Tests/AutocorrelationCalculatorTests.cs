using System.Diagnostics;
using MathNet.Numerics.Distributions;

namespace Udemy.Derivatives.Tests;

[Trait("Category", "Unit")]
public class AutocorrelationCalculatorTests(ITestOutputHelper output)
{
    [Fact]
    public void GetAutocorrelationPerLag_Sample2k_RawAndTransformed()
    {
        var sample = Normal.Samples(0, 1).Take(200_000).ToArray();

        var lagged = Enumerable.Range(2, sample.Length - 2)
            .Select(t => 0.5 * sample[t - 2] + 0.3 * sample[t - 1] + 0.2 * sample[t])
            .ToArray();

        var sw = Stopwatch.StartNew();
        var originalAutocorrelation = sample.GetAutocorrelationPerLag(30);
        var laggedAutocorrelation = lagged.GetAutocorrelationPerLag(30);
        sw.Stop();

        output.WriteLine($"spent {sw.Elapsed:g} calculating autocorrelations");

        var originalImage = PlotHelper.PlotSeries("original", originalAutocorrelation.Select((a,i) => ((decimal) i, (decimal) a)).ToArray(), false);
        var laggedImage = PlotHelper.PlotSeries("lagged", laggedAutocorrelation.Select((a,i) => ((decimal) i, (decimal) a)).ToArray(), false);

        output.WriteLine($"original sample autocorrelation: {originalImage.Path}");
        output.WriteLine($"lagged sample autocorrelation: {laggedImage.Path}");
    }
}
