using System.Text;
using FluentAssertions;
using Xunit.Abstractions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
public class KnightMovesTests(ITestOutputHelper output)
{
    [Fact]
    public void Traverse_Classics_Observe()
    {
        var hops = KnightMoves.Traverse(8, 8, new(0, 0)).ToArray();

        hops.Should().HaveCount(64, "8*8 visiting every cell once");

        var field = new int[8, 8];
        for (var step = 0; step < hops.Length; step++)
        {
            var (row, col) = hops[step];
            field[row, col] = step + 1;
        }

        var sb = new StringBuilder();
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                sb.Append($"{field[r,c]:D2}|");
            }
            output.WriteLine(sb.ToString());
            sb.Clear();
        }
    }
}