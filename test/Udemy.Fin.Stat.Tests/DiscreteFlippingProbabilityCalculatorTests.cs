namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class DiscreteFlippingProbabilityCalculatorTests(ITestOutputHelper output)
{
    [Fact]
    public async Task GetOccurrencesProbability_FairFourTosses_PlotDistribution()
    {
        const int flips = 4;

        var distribution =
            Enumerable.Range(0, flips + 1)
            .ToDictionary
            (
                c => c,
                c => DiscreteFlippingProbabilityCalculator.GetOccurrencesProbability(flips, c)
            );

        foreach (var (times, p) in distribution)
        {
            output.WriteLine($"tossing 4 times can result {times} times head in fair coin w probability {p:P2}");
        }

        await Plot("fair coin 4 times.png", distribution.Values.ToArray());
    }

    [Fact]
    public async Task GetOccurrencesProbability_BiasedFourTosses_PlotDistribution()
    {
        const int flips = 4;

        var distribution =
            Enumerable.Range(0, flips + 1)
            .ToDictionary
            (
                c => c,
                c => DiscreteFlippingProbabilityCalculator.GetOccurrencesProbability(flips, c, 0.65)
            );

        foreach (var (times, p) in distribution)
        {
            output.WriteLine($"tossing 4 times can result {times} times head in fair coin w probability {p:P2}");
        }

        await Plot("biased 65p heads coin 4 times.png", distribution.Values.ToArray());
    }

    [Fact]
    public async Task GetOccurrencesProbability_BiasedHundredTosses_PlotDistribution()
    {
        const int flips = 100;

        var distribution =
            Enumerable.Range(0, flips + 1)
            .ToDictionary
            (
                c => c,
                c => DiscreteFlippingProbabilityCalculator.GetOccurrencesProbability(flips, c, 0.65)
            );

        foreach (var (times, p) in distribution)
        {
            output.WriteLine($"tossing 4 times can result {times} times head in fair coin w probability {p:P2}");
        }

        await Plot("biased 65p heads coin 100 times.png", distribution.Values.ToArray());
    }

    private static async Task Plot<T>(string name, T[] data)
    {
        await Task.Yield();
        const int width = 3200, height = 1600;


        var plot = new ScottPlot.Plot();
        plot.Add.Scatter
        (
            data.Select((_, i) => i).ToArray(),
            data
        );
        plot.SavePng(name, width, height);
    }
}