using FluentAssertions;
using Xunit.Abstractions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
public class QuickSorterTests
{
    private readonly ITestOutputHelper _output;

    public QuickSorterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void QuickSort_Ascending_Ascending()
    {
        var source = Enumerable.Range(1, 100).ToArray();
        source.QuickSort();
        source.Should().BeInAscendingOrder();
    }

    [Fact]
    public void QuickSort_Descending_Ascending()
    {
        var source = Enumerable.Range(1, 100).Reverse().ToArray();
        source.QuickSort();
        source.Should().BeInAscendingOrder();
    }

    [Fact]
    public void QuickSort_DescendingDuplicates_Ascending()
    {
        var source = Enumerable.Range(1, 100).Reverse().ToArray();
        source = source.Concat(source).ToArray();
        source.QuickSort();
        source.Should().BeInAscendingOrder();
    }
    
    [Fact]
    public void QuickSort_Same_Same()
    {
        var source = Enumerable.Repeat(3, 100).ToArray();
        source.QuickSort();
        source.Should().OnlyContain(n => n == 3);
    }
    
    [Fact]
    public void QuickSort_SameButIncision_MoveToEnd()
    {
        var source = Enumerable.Repeat(1, 100).ToArray();
        source[50] = 10;
        source.QuickSort();
        source.SkipLast(1).Should().OnlyContain(n => n == 1);
        source.Last().Should().Be(10);
    }

    [Fact]
    public void QuickSort_Random_Ascending()
    {
        var source = Enumerable.Range(1, 100).Select(n => Random.Shared.Next(100)).ToArray();
        source.QuickSort();
        source.Should().BeInAscendingOrder();
    }

    [Fact]
    public void QuickSort_DescendingLong_Ascending()
    {
        var source = Enumerable.Range(1, 100_000_000).Reverse().ToArray();
        source.QuickSort();
        source.Should().BeInAscendingOrder();
        foreach (var threadId in QuickSorter.ThreadIds)
        {
            _output.WriteLine(threadId.ToString());
        }
    }

}