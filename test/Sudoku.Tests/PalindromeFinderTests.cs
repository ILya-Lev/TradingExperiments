using FluentAssertions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
public class PalindromeFinderTests
{
    [Fact]
    public void FindLargestPalindrome_Observe()
    {
        PalindromeFinder.FindLargestPalindrome().Should().Be(906609);
    }

    [Fact]
    public void IsPalindrome_Observe()
    {
        PalindromeFinder.IsPalindrome(906609).Should().BeTrue();
    }
}