using FluentAssertions;
using FluentAssertions.Execution;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class NormalConfidenceBoundTests
{
    [Fact]
    public void GetMuInterval_Sample1_Observe()
    {
        var sample = new[] { 1.63, -2.83, 1.14, 1.06, -0.19, 4.83, 1.25, 1.36 };
        
        var (lower, upper) = sample.GetMuInterval(confidenceLevel: 0.9, stdDev: 2.7);

        using var _ = new AssertionScope();
        lower.Should().BeApproximately(-0.19, 1e-2);
        upper.Should().BeApproximately(2.25, 1e-2);
    }

    [Fact]
    public void GetMuInterval_Sample2_Observe()
    {
        var capitalization = new double[]
        {
            1000, 1025, 1079, 1111, 1109, 1154, 1176, 1189, 1234, 1258, 1264, 1288,
            1278, 1291, 1304, 1294, 1294, 1288, 1279, 1301, 1296, 1307, 1304, 1366
        };

        var returns = capitalization.Skip(1).Zip(capitalization.SkipLast(1),
                (next, current) => next * 1.0 / current - 1)
            .ToArray();

        var (lower, upper) = returns.GetMuInterval(confidenceLevel: 0.95, stdDev: 0.03);

        using var _ = new AssertionScope();
        lower.Should().BeApproximately(0.003518, 1e-6);
        upper.Should().BeApproximately(0.024097, 1e-6);
    }

    [Fact]
    public void GetMuInterval_Sample3_Observe()
    {
        var (lower, upper) = 14.0.GetMuInterval(count: 20, confidenceLevel: 0.95, stdDev: Math.Sqrt(10));

        using var _ = new AssertionScope();
        lower.Should().BeApproximately(12.84, 1e-2);
        upper.Should().BeApproximately(15.16, 1e-2);
    }

    [Fact]
    public void GetMuInterval_Sample4_Observe()
    {
        var sample = new[] { 6.81, 4.24, 8.03, 4.48, 1.29, 6.64, 9.81, 9.51, 7.32, 8.91, 1.04, 7.62 };

        var (lower90, upper90) = sample.GetMuInterval(confidenceLevel: 0.90, stdDev: Math.Sqrt(6.0));
        var (lower99, upper99) = sample.GetMuInterval(confidenceLevel: 0.99, stdDev: Math.Sqrt(6.0));

        using var _ = new AssertionScope();
        lower99.Should().BeApproximately(4.6633, 1e-4);
        lower90.Should().BeApproximately(5.4021, 1e-4);
        upper90.Should().BeApproximately(7.2145, 1e-4);
        upper99.Should().BeApproximately(7.9533, 1e-4);
    }

    [Fact]
    public void GetMuInterval_Sample5_Observe()
    {
        var sample = new[] { -.32, 1.26, -.073, .15, 1.82, -1.58, 4.06, -1.81, 3.21 };

        var (lower90, upper90) = sample.GetMuInterval(confidenceLevel: 0.90, stdDev: Math.Sqrt(2.0));
        var (lower95, upper95) = sample.GetMuInterval(confidenceLevel: 0.95, stdDev: Math.Sqrt(2.0));
        var (lower99, upper99) = sample.GetMuInterval(confidenceLevel: 0.99, stdDev: Math.Sqrt(2.0));

        using var _ = new AssertionScope();
        lower99.Should().BeApproximately(-0.3503, 1e-4);
        lower95.Should().BeApproximately(-0.0291, 1e-4);
        lower90.Should().BeApproximately(0.1422, 1e-4);
        upper90.Should().BeApproximately(1.3505, 1e-4);
        upper95.Should().BeApproximately(1.5217, 1e-4);
        upper99.Should().BeApproximately(1.8430, 1e-4);
    }

}

