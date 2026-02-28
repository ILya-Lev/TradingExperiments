using ConcreteMath.Lib;
using FluentAssertions;

namespace ConcreteMath.Tests;

public class HanoiTowersTests(ITestOutputHelper outputHelper)
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 3)]
    [InlineData(3, 7)]
    [InlineData(4, 15)]
    [InlineData(5, 31)]
    [InlineData(6, 63)]
    [InlineData(7, 127)]
    [InlineData(8, 255)]
    [InlineData(9, 511)]
    public void Solve_N_MatchExpectedStepsNumber(int blocks, int expectedCount)
    {
        var steps = HanoiTowers.Solve(blocks).ToArray();

        steps.Should().HaveCount(expectedCount);
        for (var i = 0; i < steps.Length; i++)
        {
            var s = steps[i];
            outputHelper.WriteLine($"{i + 1}: {s.From} -> {s.To} block {s.BlockSize}");
        }
    }
}