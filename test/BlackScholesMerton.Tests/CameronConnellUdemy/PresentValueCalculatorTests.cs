using BlackScholesMerton.CameronConnellUdemy;
using FluentAssertions;
using Xunit.Sdk;

namespace BlackScholesMerton.Tests.CameronConnellUdemy;

[Trait("Category", "Unit")]
public class PresentValueCalculatorTests
{
    [Fact]
    public void GetPresentValue_ProblemSet1Task2_Observe()
    {
        var compounding = 2;
        var paymentsAndInterestByPeriod = new Dictionary<decimal, (decimal r, decimal c)>()
        {
            [0.5m] = (0.02m, 100),
            [1m] = (0.03m, 100),
            [2m] = (0.035m, 200),
            [3m] = (0.042m, 200),
        };
        
        var presentValue = PresentValueCalculator.GetPresentValue(paymentsAndInterestByPeriod, compounding);

        presentValue.Should().BeApproximately(559.2209565m, 1e-7m);
    }

    [Fact]
    public void GetBondPrice_ProblemSet1Task3_Observe()
    {
        var price = PresentValueCalculator
            .GetBondPrice(10, 0.06m, 0.04m, 2, 2);

        price.Should().BeApproximately(10.3807729m, 1e-7m);
    }
}