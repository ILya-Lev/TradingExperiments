using FluentAssertions;

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
}