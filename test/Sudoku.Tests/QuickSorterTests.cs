using FluentAssertions;
using Xunit.Abstractions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
public class QuickSorterTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void QuickSort_Ascending_Ascending(bool enableParallel, bool withRecursion)
    {
        var source = Enumerable.Range(1, 100).ToArray();
        source.QuickSort(enableParallel, withRecursion);
        source.Should().BeInAscendingOrder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void QuickSort_Descending_Ascending(bool enableParallel, bool withRecursion)
    {
        var source = Enumerable.Range(1, 100).Reverse().ToArray();
        source.QuickSort(enableParallel, withRecursion);
        source.Should().BeInAscendingOrder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void QuickSort_DescendingDuplicates_Ascending(bool enableParallel, bool withRecursion)
    {
        var source = Enumerable.Range(1, 100).Reverse().ToArray();
        source = source.Concat(source).ToArray();
        source.QuickSort(enableParallel, withRecursion);
        source.Should().BeInAscendingOrder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void QuickSort_Same_Same(bool enableParallel, bool withRecursion)
    {
        var source = Enumerable.Repeat(3, 100).ToArray();
        source.QuickSort(enableParallel, withRecursion);
        source.Should().OnlyContain(n => n == 3);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void QuickSort_SameButIncision_MoveToEnd(bool enableParallel, bool withRecursion)
    {
        var source = Enumerable.Repeat(1, 100).ToArray();
        source[50] = 10;

        source.QuickSort(enableParallel, withRecursion);

        source.SkipLast(1).Should().OnlyContain(n => n == 1);
        source.Last().Should().Be(10);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void QuickSort_Random_Ascending(bool enableParallel, bool withRecursion)
    {
        var source = Enumerable.Range(1, 100).Select(n => Random.Shared.Next(100)).ToArray();
        source.QuickSort(enableParallel, withRecursion);
        source.Should().BeInAscendingOrder();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void QuickSort_DescendingLong_Ascending(bool enableParallel, bool withRecursion)
    {
        var source = Enumerable.Range(1, 100_000_000).Reverse().ToArray();
        
        source.QuickSort(enableParallel, withRecursion);

        source.Should().BeInAscendingOrder();
        foreach (var threadId in QuickSorter.ThreadIds)
        {
            output.WriteLine(threadId.ToString());
        }
    }
}