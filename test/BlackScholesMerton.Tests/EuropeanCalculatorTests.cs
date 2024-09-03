using FluentAssertions;
using FluentAssertions.Execution;

namespace BlackScholesMerton.Tests;

[Trait("Category", "Unit")]
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

    [Fact]
    public void CreateEuropeanCalculator_PayloadInCtor_ObserveRuntime()
    {
        //code clean up in the calculator's ctor reduced runtime by 45% !
        for (int i = 0; i < 1_000_000; i++)
        {
            new EuropeanCalculator(40 + i%10, 50, 0.05, 0.2, 20 / 52.0);
        }
    }
}