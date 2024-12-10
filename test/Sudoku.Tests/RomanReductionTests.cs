using FluentAssertions;
using Xunit.Abstractions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
public class RomanReductionTests(ITestOutputHelper output)
{
    [Fact]
    public void Reduce_LongSample_CleanFormat()
    {
        RomanReduction.Reduce("LLLXXXVVVV").Should().Be("CC");
    }
}