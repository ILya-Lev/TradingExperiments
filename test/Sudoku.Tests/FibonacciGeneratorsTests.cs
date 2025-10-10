using FluentAssertions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
public class FibonacciGeneratorsTests
{
    [Fact]
    public void FibonacciSequence_Get10_MatchExpectations()
    {
        var expected = new[]{1, 1, 2, 3, 5, 8, 13, 21, 34, 55};
        FibonacciGenerators.FibonacciSequence().Take(10)
            .Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void FibonacciRatios_Get10_MatchExpectations()
    {
        var expected = new[]{1.0, .6181, .3819, .2360, .1458, 0.0901, .0557, .0344, .0212, .0131};
        FibonacciGenerators.FibonacciRatios(10, 20).Take(10)
            .Select((f, i) => (f, i))
            .Should().OnlyContain(item => Math.Abs(item.f - expected[item.i]) < 1e-4);
    }
}