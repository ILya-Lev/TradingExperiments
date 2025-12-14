using FluentAssertions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
public class QuickSorter001Tests
{
    [Fact]
    public void Sort_Simple_Asc()
    {
        var source = new[] { 1, 4, 7, 2, 8, 3, 9, 5, 6 };
        QuickSorter001.Sort(source).Should().BeInAscendingOrder();
        QuickSorter001.SortParallel(source).Should().BeInAscendingOrder();
    }
    
    [Fact]
    public void Sort_Desc_Asc()
    {
        var source = Enumerable.Range(1, 1_000_000).Reverse().ToArray();
        QuickSorter001.SortParallel(source).Should().BeInAscendingOrder();
    }

    [Fact]
    public void Sort_Asc_Asc()
    {
        var source = Enumerable.Range(1, 1_000_000).ToArray();
        QuickSorter001.Sort(source).Should().BeInAscendingOrder();
    }

    [Fact]
    public void Sort_WithDuplicates_Asc()
    {
        var source = new[] { 1, 2, 3, 4, 5, 5, 5, 5, 6, 7, 8, 9 }.Reverse().ToArray();
        QuickSorter001.Sort(source).Should().BeInAscendingOrder();
    }
    
    [Fact]
    public void Sort_Random_Asc()
    {
        var source = Enumerable.Range(1, 1_000_000).Select(i => Random.Shared.Next(0, i)).ToArray();
        QuickSorter001.Sort(source).Should().BeInAscendingOrder();
    }
}