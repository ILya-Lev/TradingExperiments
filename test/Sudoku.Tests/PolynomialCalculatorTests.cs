using FluentAssertions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
public class PolynomialCalculatorTests
{
    [Fact]
    public void GetValue_4thPower_MatchNaiveImpl()
    {
        var coefficients = new[] { 1.1124m, 2.5173m, 3.6179m, 4.1654m, 5.9274m };
        var x = 9.3216m;

        var horner = coefficients.GetValue(x);
        var straightforward = coefficients.GetValueStraightforward(x);
        var linq = coefficients.GetValueLinq(x);
        var plinq = coefficients.GetValuePlinq(x);

        horner.Should().BeApproximately(straightforward, 1e-10m);
        horner.Should().BeApproximately(linq, 1e-10m);
        plinq.Should().BeApproximately(linq, 1e-10m);
    }
}