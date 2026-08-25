using FluentAssertions;
using FluentAssertions.Execution;

namespace Udemy.FixedIncome.Tests;

[Trait("Category", "Unit")]
public class BondCalculator002Tests
{
    [Fact]
    public void GetPrice_Example43_Observe()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [0.5] = 0.006,
            [1.0] = 0.009,
            [1.5] = 0.013,
            [2.0] = 0.016,
        };

        var price = BondCalculator002
            .GetPrice(100_000, 2, 0.04, interestRates, 2);

        price.Should().BeApproximately(104_738, 1);
    }


    [Fact]
    public void GetPrice_Problem01Lecture43_Observe()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [1] = 0.003,
            [2] = 0.008,
            [3] = 0.011,
            [4] = (0.011+0.014)/2, //as linear interpolation of the 2 adjacent points
            [5] = 0.014,
        };

        var price = BondCalculator002
            .GetPrice(100_000, 5, 0.04, interestRates);

        price.Should().BeApproximately(112_617, 1);
    }

    [Fact]
    public void GetPrice_Problem02Lecture43_Observe()
    {
        var zeros = new Dictionary<double, double>()
        {
            [1] = 98,
            [2] = 94,
            [3] = 90,
            [4] = 86
        };

        var discounts = zeros.Values.Select(p => p * 1.0 / 100).ToArray();
        var f = 100_000;//face value of the bond to evaluate
        var c = f * 0.07;//coupon payment
        
        var price = discounts.Last() * f + discounts.Sum(z => z * c);
        
        price.Should().BeApproximately(111_760, 1);
    }

    [Fact]
    public void GetPrice_Problem03Lecture43_Observe()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [0.5] = 0.003,
            [1] = 0.009,
            [2] = 0.014,
            [5] = 0.018,
            [7] = 0.021,
            [10] = 0.025,
            [15] = 0.029,
            [20] = 0.031,
        };

        var interpolatedRates = interestRates.Select(p => (p.Key, p.Value))
            .Interpolate(0.5)
            .ToDictionary(p => p.t, p => p.v);

        var price = BondCalculator002
            .GetPrice(100, 10, 0.055, interpolatedRates, 2);

        price.Should().BeApproximately(127.5404, 1e-4);
    }

    [Fact]
    public void GetPrice_Problem04Lecture43_Observe()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [1] = 0.001,
            [2] = 0.007,
            [3] = 0.011,
            [5] = 0.016,
            [7] = 0.019,
            [10] = 0.021,
            [15] = 0.021,
            [20] = 0.023,
        };

        var interpolatedRates = interestRates.Select(p => (p.Key, p.Value))
            .Interpolate()
            .ToDictionary(p => p.t, p => p.v);

        var price = BondCalculator002
            .GetPrice(100, 9, 0.02, interpolatedRates);

        price.Should().BeApproximately(100.0139, 1e-4);
    }

    [Fact]
    public void GetPrice_Problem01Lecture47_Portfolio()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [0.5] = 0.006,//interpolated manually as the existing interpolator does not move beyond the first point
            [1] = 0.012,
            [2] = 0.017,
            [3] = 0.021,
            [5] = 0.024,
            [7] = 0.027,
            [10] = 0.030,
            [15] = 0.034,
            [20] = 0.036,
        };

        var interpolatedRates = interestRates.Select(p => (p.Key, p.Value))
            .Interpolate(0.5)
            .ToDictionary(p => p.t, p => p.v);

        var prices = new (int faceValue, int maturity, double coupon)[]
        {
            (100, 4, 0),//face value in thousands, i.e., 100k here
            (150, 3, 0.02),
            (140, 7, 0.05),
            (250, 20, 0.045),
        }.Select(b => BondCalculator002
            .GetPrice(b.faceValue, b.maturity, b.coupon, interpolatedRates, 2));

        prices.Sum(p => p * 1000).Should().BeApproximately(690_992.8497, 1e-4);
    }

    [Fact]
    public void GetValueUntilMaturity_ExampleLecture49_Observe()
    {
        var v0_6 = BondCalculator002.GetValueUntilMaturity(100_000, 10, 0, 0.115, 0.06);

        var tAndR = new[] { 0.04, 0.06, 0.08 }.SelectMany(r => new[] { 5, 9 }.Select(t => (t, r))).ToArray();
        var valueUntilMaturity = tAndR.Select
        (tar =>
            (
                t: tar.t,
                r: tar.r,
                v: BondCalculator002.GetValueUntilMaturity(100_000, 10, tar.t, 0.115, tar.r)
            )
        ).ToArray();

        var hs = valueUntilMaturity.Select(trv => Math.Pow(trv.v / v0_6, 1.0 / trv.t) - 1);

        var expectedValues = new double[] { 195676, 228913, 187995, 237338, 181440, 246847 };
        var expectedH = new double[] { 6.85, 5.58, 6, 6, 5.25, 6.46 }.Select(h => h / 100.0).ToArray();
        
        using var _ = new AssertionScope();
        v0_6.Should().BeApproximately(140480, 1);

        foreach (var (value, index) in valueUntilMaturity.Select((v,i) => (v.v, i)))
        {
            value.Should().BeApproximately(expectedValues[index], 1);
        }

        hs.Zip(expectedH, (h, eh) => Math.Abs(h - eh)).Should().OnlyContain(dif => dif < 1e-4);

        //valueUntilMaturity.Select(v => v.v).Should().BeEquivalentTo(
        //    expectedValues,
        //    eo => eo
        //        .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
        //        .WhenTypeIs<double>()
        //        .WithStrictOrdering()
        //);
    }
}