using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class NormalConfidenceBoundTests
{
    [Fact]
    public void GetMuBounds_Sample1_Observe()
    {
        var sample = new[] { 1.63, -2.83, 1.14, 1.06, -0.19, 4.83, 1.25, 1.36 };

        var (lower, upper) = sample.GetMuBounds(confidenceLevel: 0.9, stdDev: 2.7);

        using var _ = new AssertionScope();
        lower.Should().BeApproximately(-0.19, 1e-2);
        upper.Should().BeApproximately(2.25, 1e-2);
    }

    [Fact]
    public void GetMuBounds_Sample2_Observe()
    {
        var capitalization = new double[]
        {
            1000, 1025, 1079, 1111, 1109, 1154, 1176, 1189, 1234, 1258, 1264, 1288,
            1278, 1291, 1304, 1294, 1294, 1288, 1279, 1301, 1296, 1307, 1304, 1366
        };

        var returns = capitalization.Skip(1).Zip(capitalization.SkipLast(1),
                (next, current) => next * 1.0 / current - 1)
            .ToArray();

        var (lower, upper) = returns.GetMuBounds(confidenceLevel: 0.95, stdDev: 0.03);

        using var _ = new AssertionScope();
        lower.Should().BeApproximately(0.003518, 1e-6);
        upper.Should().BeApproximately(0.024097, 1e-6);
    }

    [Fact]
    public void GetMuBounds_Sample3_Observe()
    {
        var (lower, upper) = 14.0.GetMuBounds(count: 20, confidenceLevel: 0.95, stdDev: Math.Sqrt(10));

        using var _ = new AssertionScope();
        lower.Should().BeApproximately(12.84, 1e-2);
        upper.Should().BeApproximately(15.16, 1e-2);
    }

    [Fact]
    public void GetMuBounds_Sample4_Observe()
    {
        var sample = new[] { 6.81, 4.24, 8.03, 4.48, 1.29, 6.64, 9.81, 9.51, 7.32, 8.91, 1.04, 7.62 };

        var (lower90, upper90) = sample.GetMuBounds(confidenceLevel: 0.90, stdDev: Math.Sqrt(6.0));
        var (lower99, upper99) = sample.GetMuBounds(confidenceLevel: 0.99, stdDev: Math.Sqrt(6.0));

        using var _ = new AssertionScope();
        lower99.Should().BeApproximately(4.6633, 1e-4);
        lower90.Should().BeApproximately(5.4021, 1e-4);
        upper90.Should().BeApproximately(7.2145, 1e-4);
        upper99.Should().BeApproximately(7.9533, 1e-4);
    }

    [Fact]
    public void GetMuBounds_Sample5_Observe()
    {
        var sample = new[] { -.32, 1.26, -.073, .15, 1.82, -1.58, 4.06, -1.81, 3.21 };

        var (lower90, upper90) = sample.GetMuBounds(confidenceLevel: 0.90, stdDev: Math.Sqrt(2.0));
        var (lower95, upper95) = sample.GetMuBounds(confidenceLevel: 0.95, stdDev: Math.Sqrt(2.0));
        var (lower99, upper99) = sample.GetMuBounds(confidenceLevel: 0.99, stdDev: Math.Sqrt(2.0));

        using var _ = new AssertionScope();
        lower99.Should().BeApproximately(-0.3503, 1e-4);
        lower95.Should().BeApproximately(-0.0291, 1e-4);
        lower90.Should().BeApproximately(0.1422, 1e-4);
        upper90.Should().BeApproximately(1.3505, 1e-4);
        upper95.Should().BeApproximately(1.5217, 1e-4);
        upper99.Should().BeApproximately(1.8430, 1e-4);
    }

    [Fact]
    public void GetMuInterval_Sample6_Observe()
    {
        var sample = new[] { 9.23, -1.93, .83, 11.3, 5.22, 6.44, -.92, 3.14, -.34, -4.32, -1.83, 5.44 };

        var (lower95, upper95) = sample.GetMuInterval(confidenceLevel: 0.95, stdDev: 5);

        using var _ = new AssertionScope();
        lower95.Should().BeApproximately(-0.1406, 1e-3);
        upper95.Should().BeApproximately(5.51729, 1e-3);
        //i.e., P(-0.14 <= mu <= 5.52) = 0.95
    }

    [Fact]
    public void GetMuInterval_Sample7_Observe()
    {
        var sample = new[] { 4.23, 1.91, 3.34, 4.12, 4.34, 3.02, 3.09, 3.18, 2.81, 4.53 };

        //here we take interval => interval 0.90 means significance level 10%, interval is symmetric
        //=> take half to the left and half to the right => 1 - 0.05 = 0.95
        var (lower90, upper90) = sample.GetMuInterval(confidenceLevel: 0.90, stdDev: 1);
        var (lower95, upper95) = sample.GetMuInterval(confidenceLevel: 0.95, stdDev: 1);
        var (lower99, upper99) = sample.GetMuInterval(confidenceLevel: 0.99, stdDev: 1);

        using var _ = new AssertionScope();
        lower99.Should().BeApproximately(2.642451, 1e-3);
        lower95.Should().BeApproximately(2.837204, 1e-3);
        lower90.Should().BeApproximately(2.936851, 1e-3);
        upper90.Should().BeApproximately(3.977148, 1e-3);
        upper95.Should().BeApproximately(4.076795, 1e-3);
        upper99.Should().BeApproximately(4.271548, 1e-3);
    }

    [Fact]
    public void GetMuInterval_Sample8_Observe()
    {
        var (lower95, upper95) = 5.0.GetMuInterval(count: 15, confidenceLevel: 0.95, stdDev: 2.5);

        using var _ = new AssertionScope();
        lower95.Should().BeApproximately(3.7348, 1e-3);
        upper95.Should().BeApproximately(6.2651, 1e-3);
    }

    [Fact]
    public void GetMuInterval_Sample9_Observe()
    {
        var sample = new[] { 6.52, 5.87, 2.92, 8.43, 2.25, 5.41, 2.15, 8.92, 1.89, 5.15, 1.69, 13.12 };

        var (interval90Lower, interval90) = sample.GetMuInterval(confidenceLevel: 0.90, stdDev: 3);
        var (interval95Lower, interval95) = sample.GetMuInterval(confidenceLevel: 0.95, stdDev: 3);
        var (interval99Lower, interval99) = sample.GetMuInterval(confidenceLevel: 0.99, stdDev: 3);
        var (lower90, upper90) = sample.GetMuBounds(confidenceLevel: 0.90, stdDev: 3);
        var (lower95, upper95) = sample.GetMuBounds(confidenceLevel: 0.95, stdDev: 3);

        using var _ = new AssertionScope();
        interval99Lower.Should().BeApproximately(3.129266, 1e-3);
        interval95Lower.Should().BeApproximately(3.662621, 1e-3);
        interval90Lower.Should().BeApproximately(3.935514, 1e-3);
        lower95.Should().BeApproximately(3.935514, 1e-3);
        lower90.Should().BeApproximately(4.250143, 1e-3);
        upper90.Should().BeApproximately(6.469856, 1e-3);
        upper95.Should().BeApproximately(6.784485, 1e-3);
        interval90.Should().BeApproximately(6.784485, 1e-3);
        interval95.Should().BeApproximately(7.057378, 1e-3);
        interval99.Should().BeApproximately(7.590733, 1e-3);
    }

    [Fact]
    public void GetMuBounds_Sample10_Observe()
    {
        var capitalization = new double[]
        {
            1000, 1025, 1079, 1111, 1109, 1154, 1176, 1189, 1234, 1258, 1264, 1288,
            1278, 1291, 1304, 1294, 1294, 1288, 1279, 1301, 1296, 1307, 1304, 1366
        };

        var returns = capitalization.Skip(1).Zip(capitalization.SkipLast(1),
                (next, current) => next * 1.0 / current - 1)
            .ToArray();

        var netReturns = capitalization.Skip(1).Zip(capitalization.SkipLast(1),
                (next, current) => next * 1.0 / current)
            .ToArray();

        var logReturns = capitalization.Skip(1).Zip(capitalization.SkipLast(1),
                (next, current) => Math.Log(next * 1.0 / current))
            .ToArray();

        var (lowerReturns95, upperReturns95) = returns.GetMuInterval(confidenceLevel: 0.95, stdDev: 0.03);
        var (lowerNetReturns95, upperNetReturns95) = netReturns.GetMuInterval(confidenceLevel: 0.95, stdDev: 0.03);
        var (lowerLogReturns95, upperLogReturns95) = logReturns.GetMuInterval(confidenceLevel: 0.95, stdDev: 0.03);
        var (lowerReturns99, upperReturns99) = returns.GetMuInterval(confidenceLevel: 0.99, stdDev: 0.03);
        var (lowerNetReturns99, upperNetReturns99) = netReturns.GetMuInterval(confidenceLevel: 0.99, stdDev: 0.03);
        var (lowerLogReturns99, upperLogReturns99) = logReturns.GetMuInterval(confidenceLevel: 0.99, stdDev: 0.03);

        //intervals/bounds for returns and log returns are very close; net returns are off by 1.
        using var _ = new AssertionScope();
        lowerReturns95.Should().BeApproximately(0.00154692799, 1e-6);
        lowerNetReturns95.Should().BeApproximately(1.00154692, 1e-6);
        lowerLogReturns95.Should().BeApproximately(0.00129987, 1e-6);
        upperNetReturns95.Should().BeApproximately(1.02606777, 1e-6);
        upperLogReturns95.Should().BeApproximately(0.02582071, 1e-6);
        upperReturns95.Should().BeApproximately(0.02606777, 1e-6);

        lowerReturns99.Should().BeApproximately(-0.0023055, 1e-6);
        lowerNetReturns99.Should().BeApproximately(0.997694424, 1e-6);
        lowerLogReturns99.Should().BeApproximately(-0.00255263, 1e-6);
        upperNetReturns99.Should().BeApproximately(1.029920276, 1e-6);
        upperLogReturns99.Should().BeApproximately(0.029673220, 1e-6);
        upperReturns99.Should().BeApproximately(0.02992, 1e-6);
    }


    [Fact] 
    public void GetPValue_Sample1_Observe()
    {
        var sample = new[]
        {
            12.25, 8.48, 4.55, 8.14, 8.34, 8.82, 6.68, 8.32, 12.92, 5.92,
            6.62, 4.0, 11.10, 10.19, 6.63, 3.23, 2.65, 8.11, 13.12, 6.32
        };

        var (z, p) = sample.GetPValue(6.5, Math.Sqrt(7));
        var c = 0.05.GetCriticalRegion();

        using var _ = new AssertionScope();
        z.Should().BeApproximately(2.230362, 1e-3);
        p.Should().BeApproximately(0.987138, 1e-6);
        c.Should().BeApproximately(1.644853, 1e-3);
        //as z > c => reject the null hypothesis (that mu = 6.5)
    }
}

