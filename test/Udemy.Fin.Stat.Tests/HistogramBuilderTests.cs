using FluentAssertions;
using FluentAssertions.Execution;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class HistogramBuilderTests(ITestOutputHelper output)
{
    [Fact]
    public void GetHistogramDefinition_Sample001_Observe()
    {
        var data = new[] { -0.94, 1.56, 2.23, 0.65, 1.34, 1.87, 0.32, -0.23, -1.21, -1.15, -2.52, 2.29, -0.02, 1.24 };

        var ofWidthOne = data.GetHistogramDefinition(1.0).ToArray();
        var ofWidthQuarter = data.GetHistogramDefinition(0.25).ToArray();

        using var _ = new AssertionScope();
        ofWidthOne.Should().HaveCount(5);
        ofWidthOne.Select(p => p.count).Should().BeEqualTo([1, 3, 3, 3, 4]);
        ofWidthQuarter.Should().HaveCount(20);
        ofWidthQuarter.Select(p => p.count).Should().BeEqualTo([1, 0, 0, 0, 0, 2, 1, 0, 0, 1, 1, 1, 1, 0, 0, 2, 1, 1, 0, 2]);
        ofWidthOne.Sum(p => p.count).Should().Be(ofWidthQuarter.Sum(p => p. count)).And.Be(data.Length);
    }

    [Fact]
    public void GetHistogramDefinition_Sample001_StartM3_Observe()
    {
        var data = new[] { -0.94, 1.56, 2.23, 0.65, 1.34, 1.87, 0.32, -0.23, -1.21, -1.15, -2.52, 2.29, -0.02, 1.24 };

        var ofWidthOne = data.GetHistogramDefinition(-3, 1.0).ToArray();
        var ofWidthQuarter = data.GetHistogramDefinition(-3, 0.25).ToArray();

        using var _ = new AssertionScope();
        ofWidthOne.Should().HaveCount(6);
        ofWidthOne.Select(p => p.count).Should().BeEqualTo([1, 2, 3, 2, 4, 2]);
        ofWidthQuarter.Should().HaveCount(22);
        ofWidthQuarter.Select(p => p.count).Should().BeEqualTo([0, 1, 0, 0, 0, 0, 0, 2, 1, 0, 0, 2, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1]);
        ofWidthOne.Sum(p => p.count).Should().Be(ofWidthQuarter.Sum(p => p.count)).And.Be(data.Length);
    }

    [Fact]
    public async Task GetHistogramDefinition_SnP500_TryDifferentBinWidth()
    {
        var start = DateOnly.ParseExact("20090301", "yyyyMMdd");
        var end = DateOnly.ParseExact("20191231", "yyyyMMdd");
        var filter = (DateOnly d) => start <= d && d <= end;

        var prices = await DemoHelpers.LoadClosePrices("GSPC", filter).ContinueWith(t => t.Result.ToArray());

        var logReturns = prices.Skip(1).Zip(prices.SkipLast(1), (n, c) => Math.Log(n) - Math.Log(c)).ToArray();

        var binWidth = logReturns.GetBinWidthQuantileRange();
        output.WriteLine($"Freedman-Diaconis rule gives {binWidth}, while Scott's rule {logReturns.GetBinWidthStandardDeviation()}");

        var histogram = logReturns.GetHistogramDefinition(binWidth).ToArray();
        foreach (var (bucketStart, count) in histogram)
        {
            output.WriteLine($"{bucketStart}; {count}");
        }
    }
}
