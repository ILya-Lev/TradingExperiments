using FluentAssertions;
using FluentAssertions.Execution;

namespace BlackScholesMerton.Tests;

public class BinomialTreeModelTests
{
    [Fact]
    public void Ctor_AmericanPutOption_MatchManualCalculations()
    {
        var treeModel = new BinomialTreeModel(false, 2, 100, 1.1, 0.9, 1 / 1.05
                                            , (s) => Math.Max(0, 101 - s));
        
        using var scope = new AssertionScope();
        treeModel.Q.Should().BeApproximately(0.75, 1e-2);
        treeModel.Root.Payoff.Should().BeApproximately(2.9592, 1e-4);
    }

    [Fact]
    public void Ctor_AmericanStrikeSquaredOption_MatchManualCalculations()
    {
        var treeModel = new BinomialTreeModel(false, 2, 100, 1.02, 0.98,1 / 1.01
                                            , (s) => s > 101 ? s*s : 0);
        
        using var scope = new AssertionScope();
        treeModel.Q.Should().BeApproximately(0.75, 1e-2);
        treeModel.Root.Payoff.Should().BeApproximately(7725.7426, 1e-4);
    }
}