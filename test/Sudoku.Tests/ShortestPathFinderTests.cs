using FluentAssertions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
public class ShortestPathFinderTests
{
    [Fact]
    public void GetShortestPath_5Nodes_ShortestPathMaxHops()
    {
        var a = new ShortestPathFinder.Node("A");
        var b = new ShortestPathFinder.Node("B");
        var c = new ShortestPathFinder.Node("C");
        var d = new ShortestPathFinder.Node("D");
        var e = new ShortestPathFinder.Node("E");

        a.Next[b] = 1;
        a.Next[c] = 2;
        
        b.Next[d] = 4;
        b.Next[e] = 5;

        c.Next[d] = 1;
        c.Next[e] = 4;

        d.Next[e] = 1;

        var report = ShortestPathFinder.GetShortestPath(a, e);

        report.Should().Be("A -> C -> D -> E total length 4");
    }
}