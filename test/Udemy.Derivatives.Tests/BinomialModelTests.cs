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