using FluentAssertions;
using FluentAssertions.Execution;

namespace Udemy.FixedIncome.Tests;

[Trait("Category", "Unit")]
public class LinearInterpolatorTests(ITestOutputHelper output)
{
    [Fact]
    public void Interpolate_2Gaps_CoverBoth()
    {
        var discountFactorsPerYear = new[] { (1, 11.9 / 12), (2, 19.4 / 20), (4, 30.5 / 33), (7, 42.7 / 50) };
        var interestRatesPerYear = discountFactorsPerYear.Select(z => (z.Item1 * 1.0, Math.Pow(z.Item2, -1.0 / z.Item1) - 1))
            .ToArray();

        var interpolatedInterestRatesPerYear = interestRatesPerYear.Interpolate().ToArray();
        var zs = interpolatedInterestRatesPerYear.Select(r => Math.Pow(1 + r.v, -r.t)).ToArray();

        var p = 100_000 * (zs[1] + zs[2] + zs[3] + zs[4]);//i.e., z(2) + z(3) + z(4) + z(5)

        p.Should().BeApproximately(374_511.3985, 1e-4);
        output.WriteLine($"portfolio price is {p:n4}");
        for (var i=0;i<zs.Length;i++)
        {
            output.WriteLine($"Z({i+1}) = {zs[i]:N4}");
        }
    }

    [Fact]
    public void GetAnnuityPrice_LinearInterestRate_Observe()
    {
        var rs = new[] { 0.003, 0.008, 0.011, 0.0125, 0.014, 0.015, 0.016 };

        var discount = 1.0 / (1 + rs[5]);
        for (var i = 4; i >= 0; i--)
        {
            discount += 1;
            discount *= 1.0 / (1 + rs[i]);
        }
        var realPrice = 10_000 * discount;//incorrect aproach

        var theoreticalPrice = 10_000 * rs.SkipLast(1).Select((r,t) => Math.Pow(1+r, -t-1)).Sum(); //correct aproach

        using var _ = new AssertionScope();
        realPrice.Should().BeApproximately(58224.4453, 1e-4);
        theoreticalPrice.Should().BeApproximately(57478.2490, 1e-4);
    }
}