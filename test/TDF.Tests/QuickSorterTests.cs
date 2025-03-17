using FluentAssertions;
using TDF.Lib;

namespace TDF.Tests;

public class QuickSorterTests
{
    private static readonly Func<int, int, bool> _isBefore = (a, b) => a < b;
    [Fact]
    public async Task Sort_DescendingShort_Ascending()
    {
        var source = new[] {1, 4, 5, 6, 8, 9, 17, 11, 8, 12}
            .OrderByDescending(n => n)
            .ToArray();

        var sorter = new QuickSorterAsync<int>(_isBefore);

        var sorted = await sorter.Sort(source);

        sorted.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Sort_Random_Ascending()
    {
        var source = Enumerable.Range(1, 10_000_000)
            .Select(n => Random.Shared.Next(n + 10))
            .ToArray();

        var sorter = new QuickSorterAsync<int>(_isBefore);

        var sorted = await sorter.Sort(source);

        sorted.Should().BeInAscendingOrder();
    }
}