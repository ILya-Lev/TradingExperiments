using FluentAssertions;
using Xunit.Abstractions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
public class HanoiTowersTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void Solve_Small_AlmostPowerOf2(int n)
    {
        ulong expectedStepsNumber = (1UL << n) - 1;
        ulong counter = 0;

        foreach (var step in HanoiTowers.Solve(n))
        {
            counter++;
            output.WriteLine(HanoiTowers.Materialize(step));
        }

        counter.Should().Be(expectedStepsNumber);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(23)]
    [InlineData(25)]
    //[InlineData(30)]
    public void Solve_Medium_AlmostPowerOf2(int n)
    {
        long expectedStepsNumber = (1L << n) - 1;
        long counter = HanoiTowers.Solve(n).LongCount();

        counter.Should().Be(expectedStepsNumber);
    }
}