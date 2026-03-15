using FluentAssertions;
using static Udemy.Fin.Stat.DiscreteBinomialDistributionCalculator;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class DiscreteBinomialDistributionCalculatorTests(ITestOutputHelper output)
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
                c => GetOccurrencesProbability(flips, c)
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
                c => (double)c,
                c => GetOccurrencesProbability(flips, c, 0.65)
            );

        foreach (var (times, p) in distribution)
        {
            output.WriteLine($"tossing 4 times can result {times} times head in fair coin w probability {p:P2}");
        }

        await Plot("biased 65p heads coin 4 times.png", distribution.Values.ToArray());
        await Plot("cdf biased 65p heads coin 4 times.png", distribution.ConvertDistributionIntoCdf().Values.ToArray());
    }

    [Fact]
    public async Task GetOccurrencesProbability_BiasedHundredTosses_PlotDistribution()
    {
        const int flips = 100;

        var distribution =
            Enumerable.Range(0, flips + 1)
            .ToDictionary
            (
                c => (double)c,
                c => GetOccurrencesProbability(flips, c, 0.65)
            );

        foreach (var (times, p) in distribution)
        {
            output.WriteLine($"tossing 4 times can result {times} times head in fair coin w probability {p:P2}");
        }

        await Plot("biased 65p heads coin 100 times.png", distribution.Values.ToArray());
        await Plot("cdf biased 65p heads coin 100 times.png", distribution.ConvertDistributionIntoCdf().Values.ToArray());
    }

    [Fact]
    public void GetUpToProbability_Bin10p30_UpTo3_Observe()
    {
        var probabilityOfSuccessUpTo3Times = GetUpToProbability(10, 3, 0.3);

        probabilityOfSuccessUpTo3Times.Should().BeApproximately(.6496, 1e-4);
    }

    [Fact]
    public void GetUpToProbability_Bin10p10_UpTo2_Observe()
    {
        var probabilityOfSuccessUpTo3Times = GetUpToProbability(10, 2, 0.1);

        probabilityOfSuccessUpTo3Times.Should().BeApproximately(.9298, 1e-4);
    }

    [Fact]
    public void GetUpToProbability_Bin10p0094_UpTo1_Observe()
    {
        var probabilityOfSuccessUpTo3Times = GetUpToProbability(10, 1, 0.022568);

        probabilityOfSuccessUpTo3Times.Should().BeApproximately(.9796, 1e-4);
    }

    [Fact]
    public void GetUpToProbability_Bin50p04_UpTo2_Observe()
    {
        var upToTwiceBySampleSize = Enumerable.Range(1,5)
            .Select(multiplier => 50*multiplier)
            .Select(sampleSize => (sampleSize, p: GetUpToProbability(sampleSize, 2, 0.04)))
            .ToArray();
        
        var upToOnceBySampleSize = Enumerable.Range(1,5)
            .Select(multiplier => 50*multiplier)
            .Select(sampleSize => (sampleSize, p: GetUpToProbability(sampleSize, 1, 0.04)))
            .ToArray();

        upToTwiceBySampleSize.Single(item => item.sampleSize == 50).p.Should().BeApproximately(.6767, 1e-4);
        
        output.WriteLine($"up to 2 occurrences");
        foreach (var (sampleSize, p) in upToTwiceBySampleSize)
            output.WriteLine($"{sampleSize} -> {p:P2}");

        output.WriteLine($"up to 1 occurrences");
        foreach (var (sampleSize, p) in upToOnceBySampleSize)
            output.WriteLine($"{sampleSize} -> {p:P2}");
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