using FluentAssertions;
using FluentAssertions.Execution;

namespace BlackScholesMerton.Tests;

public class EuropeanCalculatorTests
{
    private const double Precision = 5e-5;

    [Fact]
    public void GetPricesAndGreeks_HullBookExample_MatchManualCalculations()
    {
        var sut = new EuropeanCalculator(49, 50, 0.05, 0.2, 20/52.0);
        
        using var scope = new AssertionScope();

        sut.CallPrice.Should().BeApproximately(2.4005, Precision);
        sut.PutPrice.Should().BeApproximately(2.4482, Precision);
        
        sut.CallDelta.Should().BeApproximately(0.5216, Precision);
        sut.PutDelta.Should().BeApproximately(-0.4784, Precision);
        
        sut.CallTheta.Should().BeApproximately(-4.3053, Precision);
        sut.PutTheta.Should().BeApproximately(-1.85295, Precision);
        
        sut.Gamma.Should().BeApproximately(0.0655, Precision);
        
        sut.Vega.Should().BeApproximately(12.1055, Precision);
        
        sut.CallRho.Should().BeApproximately(8.90695, Precision);
        sut.PutRho.Should().BeApproximately(-10.34796, Precision);
    }
}