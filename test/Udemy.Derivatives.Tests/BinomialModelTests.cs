using FluentAssertions;

namespace Udemy.Derivatives.Tests;

[Trait("Category", "Unit")]
public class BinomialModelTests(ITestOutputHelper output)
{
    [Fact]
    public void GetInitialDerivativePrice_2Levels_MatchManualCalculations()
    {
        var sut = new BinomialModel(1.2, .9, .06);
        var d0 = sut.GetInitialDerivativePrice([-15, 10, 25]);
        d0.Should().BeApproximately(5.478, 1e-3);
    }

    [Fact]
    public void GetInitialDerivativePrice_2LevelsProblem3_MatchManualCalculations()
    {
        var sut = new BinomialModel(1.3, .9, .04);
        var d0 = sut.GetInitialDerivativePrice([30, 15, 25]);
        d0.Should().BeApproximately(19.47, 1e-2);
    }

    [Fact]
    public void GetInitialDerivativePrice_2LevelsProblem4_PutCallParityPreserved()
    {
        var sut = new BinomialModel(1.2, .8, .07);
        
        var c0 = sut.GetInitialDerivativePrice([34, 0, 0]);
        var p0 = sut.GetInitialDerivativePrice([0, 14, 46]);
        var f0 = sut.GetInitialDerivativePrice([34, -14, -46]);

        (c0 - p0).Should().BeApproximately(f0, 1e-2);
        output.WriteLine($"initial call {c0:N4}, put {p0:N4}, forward {f0:N4}");
        output.WriteLine($"S(0) - K/(1+r)^2 = {100 - 110/1.07/1.07}");
    }

    [Fact]
    public void GetInitialDerivativePrice_1Level_MatchManualCalculations()
    {
        var sut = new BinomialModel(1.16, .88, .03);
        var d0 = sut.GetInitialDerivativePrice([20, 10]);
        d0.Should().BeApproximately(14.91, 1e-2);
    }

    [Fact]
    public void GetInitialDerivativePrice_1LevelProblem6_MatchManualCalculations()
    {
        var sut = new BinomialModel(1.1, .925, .05);
        var d0 = sut.GetInitialDerivativePrice([0, 8]);
        d0.Should().BeApproximately(2.177, 1e-3);
    }
}